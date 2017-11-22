using ConsoleMars2000Server.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Implementation {
	public class ClientSendingGroup : IClientSendingGroup {

		Dictionary<object, IClient> _clients = new Dictionary<object, IClient>();
		
		SemaphoreSlim _enterSemaphore = new SemaphoreSlim(1);
		volatile bool _isClosed = false;
        System.Collections.Concurrent.ConcurrentQueue<Task> _sendingTask = new System.Collections.Concurrent.ConcurrentQueue<Task>();

		public event Action OnZero;
		public event Func<IClient, Task> OnNewClient;

		public async Task<bool> TryAddClient(IClient client) {
			await _enterSemaphore.Lock(() => {
				if (_isClosed) return;
				_clients.Add(client, client);
			});
			if (_isClosed) return false;
			await OnNewClient?.Invoke(client);
			return true;
		}

		public async Task RemoveClient(IClient clientState) {
            bool zero = false;
            await _enterSemaphore.Lock(() => {
				_clients.Remove(clientState);
				if (_clients.Count == 0) {
					zero = true;

				}
			});
			if (zero) OnZero?.Invoke();
		}

		public void Close() {
			_isClosed = true;
		}

		public Task SendAllFrom(IClient from, byte[] packet) {
			return SendAllFrom(from, packet, packet.Length);
		}

        public Task SendAll(byte[] packet) {
            return SendArraySegmentToAll(new ArraySegment<Byte>(packet, 0, packet.Length));
        }

        public Task SendAllFrom(IClient from, byte[] packet, int totalPacketSize) {
			return SendArraySegmentToAllFrom(from, new ArraySegment<Byte>(packet, 0, totalPacketSize));
		}

		async Task SendArraySegmentToAllFrom(IClient from, ArraySegment<Byte> packet) {
            await _enterSemaphore.Lock(() => {
				if (_clients.ContainsKey(from)) {
					var cur = _clients.GetEnumerator();
					while (cur.MoveNext()) {
						var el = cur.Current;
						if (el.Value != from) {
                            var sendTask =  el.Value.Send(packet);
                            _sendingTask.Enqueue(sendTask);
                        }
					}
				}
			});
            
            while (_sendingTask.TryDequeue(out Task task)) {
                await task;
            }
        }

        async Task SendArraySegmentToAll(ArraySegment<Byte> packet) {
            await _enterSemaphore.Lock(() => {
                var cur = _clients.GetEnumerator();
                while (cur.MoveNext()) {
                    var el = cur.Current;
                    var sendTask = el.Value.Send(packet);
                    _sendingTask.Enqueue(sendTask);
                }
            });
            while (_sendingTask.TryDequeue(out Task task)) {
                await task;
            }
        }
    }

}
