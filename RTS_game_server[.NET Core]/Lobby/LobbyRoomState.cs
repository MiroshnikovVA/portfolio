using ConsoleMars2000Server.Abstractions;
using ConsoleMars2000Server.Game;
using SpaceCrabDevelopment.Network;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleMars2000Server.Implementation;

namespace ConsoleMars2000Server.Lobby {
	public class LobbyRoomState : IRoomState {
		private IClientSendingGroup _clientContainer;
		private IHostRoom _hostRoom;

		volatile int _nextRoomId = 0;
		ConcurrentDictionary<int, RoomAdapter> _rooms = new ConcurrentDictionary<int, RoomAdapter>();
        List<LobbyRoomsListResponcePacket.RoomRecord> _listRoomsRecords = new List<LobbyRoomsListResponcePacket.RoomRecord>();
        volatile bool _roomsChanged = false;
        SemaphoreSlim _sendRoomsSemaphore = new SemaphoreSlim(1);
        private byte[] _oldRoomsBytes;

        public LobbyRoomState(IClientSendingGroup clientContainer, IHostRoom hostRoom) {
			_hostRoom = hostRoom;
			_clientContainer = clientContainer;
		}

		public async Task AddPacket(IClient client, byte[] packet, int totalPacketSize, MemoryStream stream, BinaryReader reader) {
			var tag = (LobbyPackets)reader.ReadByte();
			switch (tag) {
				case LobbyPackets.LobbyCreateRoomRequest:
					await CreateRoom(new LobbyCreateRoomRequestPacket(reader), client);
					break;
				case LobbyPackets.LobbyEnterRoomRequest:
					await EnterRoom(new LobbyEnterRoomRequestPacket(reader), client);
					break;
                case LobbyPackets.LobbyRoomsList:
                    await SendRooms();
                    break;
                default:
					break;
			}
		}

        async Task SendRoomsOneClient(IClient client) {
            var bytes = _oldRoomsBytes;
            if (bytes == null || _roomsChanged) {
                await SendRooms();
            }
            else {
                await client.Send(bytes);
            }
        }

        async Task SendRooms() {
            if (_roomsChanged) {
                await _sendRoomsSemaphore.Lock(() => {
                    if (_roomsChanged) {
                        _roomsChanged = false;
                        var noWait = _clientContainer.SendAll(GetRoomsBytes());
                    }
                });
            }
        }

        byte[] GetRoomsBytes() {
            _listRoomsRecords.Clear();
            foreach (var room in _rooms.Values) {
                _listRoomsRecords.Add(room.PacketRecord);
            }
            var bytes = new LobbyRoomsListResponcePacket(_listRoomsRecords).GetBytesTotal();
            _oldRoomsBytes = bytes;
            return bytes;
        }

        async Task CreateRoom(LobbyCreateRoomRequestPacket packet, IClient client) {
			IRoom room = new GameRoom(_hostRoom, System.Threading.Interlocked.Increment(ref _nextRoomId), packet.Description);
            LobbyRoomsListResponcePacket.RoomRecord record = new LobbyRoomsListResponcePacket.RoomRecord() {
                Name = packet.RoomName,
                Id = room.Id,
            };
            RoomAdapter adapter = new RoomAdapter() { Name = packet.RoomName, Room = room, Creator = client, PacketRecord = record };
			if (!await room.TryEnter(client)) {
				await client.Send(new EnterGameRoomResponcePacket(requestId: packet.RequestId, success: false).GetBytesTotal()); //отправляем пакет с отказом
				//await SendRooms(); //Обновляем всем список комнат
			}
			else {
				_rooms[adapter.Room.Id] = adapter;
                _roomsChanged = true;
            }
		}

        public void OnRemoveChildrenRoom(IRoom room) {
            _rooms.TryRemove(room.Id, out RoomAdapter roomAdapter);
            _roomsChanged = true;
        }

		async Task EnterRoom(LobbyEnterRoomRequestPacket packet, IClient client) {
			if (_rooms.TryGetValue(packet.RoomId, out RoomAdapter adapter) && await adapter.Room.TryEnter(client)) {
				return;
			} else {
				await client.Send(new EnterGameRoomResponcePacket(requestId: packet.RequestId, success: false).GetBytesTotal()); //отправляем пакет с отказом
				await SendRooms(); //Обновляем всем список комнат
			}
		}

		async Task IRoomState.OnNewClient(IClient client) {
			await client.Send(new LobbyInitPacket(client.ID).GetBytesTotal());
            await SendRoomsOneClient(client);
			Console.WriteLine($"New client [{client.ID}] in lobby");
        }

		class RoomAdapter {
			public IRoom Room;
			public string Name;
			public IClient Creator;
            public LobbyRoomsListResponcePacket.RoomRecord PacketRecord;
        }
	}
}
