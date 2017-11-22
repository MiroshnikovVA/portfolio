using ConsoleMars2000Server.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Implementation {
	public class Client : IClient {

		Socket _client;
        //Обеспечивает целостность пакетов при отправке
        SemaphoreSlim _sendingSemaphore = new SemaphoreSlim(1);
        IClientReader _clientReader;
        object _setReaderTranzactionObject = new object();
        volatile bool _changingRoomStarted;
        //Даная очередь используется для хранения отложенных пакетов отправки
        ConcurrentQueue<ArraySegment<byte>> _lockQueueOfSendingMessage = new ConcurrentQueue<ArraySegment<byte>>();

		public int ID { get; private set; }

		public string PlayerName { get { return $"Player{ID}"; } }

		public Client(Socket client, int id) {
			_client = client;
			ID = id;
        }

        //public async Task Transaction(Func<Task> action) {
        //    try {
        //        BeginChangingRoomTransaction();
        //        await action();
        //    } finally {
        //        FinallyChangingRoomTransaction();
        //    }
        //}

        public void BeginChangingRoomTransaction() {
            if (_changingRoomStarted) throw new InvalidOperationException($"Transaction is already started ");
            _changingRoomStarted = true;
        }

		public void FinallyChangingRoomTransaction() {
            var noAwait = ChangingComplete();
        }

        public void ChangeRoom(IClientReader newClientReader) {
            if (!_changingRoomStarted) throw new InvalidOperationException($"{nameof(ChangeRoom)}() can call only in Transaction(()=>... )");
            if (newClientReader != _clientReader) {
				FireDisconnect();
				_clientReader = newClientReader;
            }
        }

        public Task Send(byte[] sendMessage) {
			var sendMessageBuf = new ArraySegment<Byte>(sendMessage, 0, sendMessage.Length);
			return Send(sendMessageBuf);
		}

		public async Task Send(ArraySegment<byte> sendMessage) {
			await _sendingSemaphore.Lock(async () => {
				if (_changingRoomStarted) {
					//Если отправка пока заблокирована, то просто выждем, добавив сообщения в очередь
					//Однако данные в массиве уже могут быть изменены, так что придется скопировать их
					var arrayCopy = new byte[sendMessage.Count];
					Array.ConstrainedCopy(sendMessage.Array, sendMessage.Offset, arrayCopy, 0, sendMessage.Count);
					_lockQueueOfSendingMessage.Enqueue(new ArraySegment<byte>(arrayCopy));
				}
				else {
					await AtomicSend(sendMessage);
				}
			});
		}

		async Task ChangingComplete() {
            if (_changingRoomStarted) {
                await _sendingSemaphore.Lock(async () => {
                    if (_changingRoomStarted) {
                        _changingRoomStarted = false;
                        //Отправляем задержанные ранее сообщения
                        while (_lockQueueOfSendingMessage.TryDequeue(out var queueMessage)) {
                            await AtomicSend(queueMessage);
                        }
                    }
                });
            }
        }

        async Task AtomicSend(ArraySegment<byte> sendMessage) {
            try {
                while (sendMessage.Count > 0) {
                    var c = await _client.SendAsync(sendMessage, SocketFlags.None);
                    Helper.AddOffset(ref sendMessage, c);
                }
            }
            catch (Exception excep) {
                Console.WriteLine($"Excepltion send to client {ID}: {excep.Message}");
				FireDisconnect();
            }
        }

		public async Task TCPSocketWork() {
			Socket client = _client;
			const int LengthSize = 4;
			var bufferLength = LengthSize;
			var bufferArray = new byte[LengthSize];
			bool noError = false;
			var reciveMemoryStream = new MemoryStream(bufferArray);
			var reciveBinaryReader = new BinaryReader(reciveMemoryStream);
			try {
				for (;;) {
					var reciveBuf = new ArraySegment<Byte>(bufferArray, 0, LengthSize);
					while (reciveBuf.Count > 0) {
						var c = await client.ReceiveAsync(reciveBuf, SocketFlags.None);
						if (c == 0) {
							noError = true;
							Console.WriteLine("Client disconnected");
							throw new SocketException();
						}
						Helper.AddOffset(ref reciveBuf, c);
					}
					var packetSize = BitConverter.ToInt32(bufferArray, 0);
					var totalPacketSize = packetSize + LengthSize;
					if (bufferLength < totalPacketSize) {
						var oldBuf = bufferArray;
						bufferArray = new byte[totalPacketSize];
						Array.Copy(oldBuf, bufferArray, 4);
						bufferLength = totalPacketSize;
						reciveBinaryReader.Dispose();
						reciveMemoryStream = new MemoryStream(bufferArray);
						reciveBinaryReader = new BinaryReader(reciveMemoryStream);
					}
					reciveBuf = new ArraySegment<Byte>(bufferArray, LengthSize, packetSize);
					while (reciveBuf.Count > 0) {
						var c = await client.ReceiveAsync(reciveBuf, SocketFlags.None);
						if (c == 0) throw new SocketException();
						Helper.AddOffset(ref reciveBuf, c);
					}
					reciveMemoryStream.Position = LengthSize;
					await PacketParse(bufferArray, totalPacketSize, reciveMemoryStream, reciveBinaryReader);
				}
			}
			catch (Exception excep) {
				if (!noError) {
					Console.WriteLine($"Error recive client {excep.Message}");
					throw;
				}
			}
			finally {
				FireDisconnect();
				Console.WriteLine($"Complete recive client {ID}");
				reciveBinaryReader.Dispose();
			}
		}

		void FireDisconnect() {
			_clientReader?.OnClientDisconnect(this);
		}

		Task PacketParse(byte[] packet, int totalPacketSize, MemoryStream stream, BinaryReader reader) {
            var pacrcer = _clientReader;
            return pacrcer.OnClientRecivePacket(this, packet, totalPacketSize, stream, reader);
		}
	}
}
