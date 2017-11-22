using ConsoleMars2000Server.Abstractions;
using ConsoleMars2000Server.Implementation;
using SpaceCrabDevelopment.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ConsoleMars2000Server.Game {
	public class GameRoomState : IRoomState {

		

		SemaphoreSlim _addUnitSemaphore = new SemaphoreSlim(1);
		ConcurrentDictionary<int, Unit> _units = new ConcurrentDictionary<int, Unit>();
		IClientSendingGroup _sendingGroup;
		

		private LobbyCreateRoomRequestPacket.RoomDescription _description;
		private RoomSlots _slots;
		private bool _gameStarted;
		private ConcurrentDictionary<int, int> _slotsColor = new ConcurrentDictionary<int, int>();

		public async Task AddPacket(IClient client, byte[] packet, int totalPacketSize, MemoryStream stream, BinaryReader reader) {
			var tag = (GamePackets)reader.ReadByte();
			bool transitPackage = false;
			switch (tag) {
				case GamePackets.TopDownUnitMove:
					TopDownUnitMoveHandler(new UnitTopDownPositionPacket(reader), client, ref transitPackage);
					break;
				case GamePackets.UnitCreate:
					await AsyncAddUnit(new CreateUnitPacket(reader), client, ref transitPackage);
					break;
				case GamePackets.ClientReady:
					await ClientReadyHandler(new ClientReadyPacket(reader), client, ref transitPackage);
					break;
				case GamePackets.SelectSlotRequestPacket:
					await SelectSlotRequest(new TrySelectSlotRequestPacket(reader), client, ref transitPackage);
					break;
				case GamePackets.StartPlay:
					await TrySendStart();
					break;
				case GamePackets.ChangeColor:
					transitPackage = false;
					await ChangeColor(new ChangeColorPacket(reader), client);
					break;
				default:
					transitPackage = true;
					break;
			}
			if (transitPackage) {
				await _sendingGroup.SendAllFrom(client, packet, totalPacketSize);
				//Console.WriteLine($"transit Package {BitConverter.ToString(packet, 0, totalPacketSize)}");
			}
		}

		async Task ChangeColor(ChangeColorPacket changeColorPacket, IClient client) {
			await _slots.ClientSetColor(client, changeColorPacket.SlotId, changeColorPacket.ColorIndex, changeColorPacket.SubColorIndex);
		}

		async Task<bool> TrySendStart() {
			if (!_gameStarted) {
				if (await _slots.IsAllReady()) {
					_gameStarted = true;
					var noawait = _sendingGroup.SendAll(new StartPlayPacket().GetBytesTotal());
					return true;
				}
			}
			return false;
		}

		private Task SelectSlotRequest(TrySelectSlotRequestPacket packet, IClient client, ref bool transitPackage) {
			transitPackage = false;
			return _slots.ClientSelectSlot(client, packet.SlotId);
		}

		public async Task RemoveClient(IClient client) {
			await _slots.RemoveClient(client);
			await _sendingGroup.SendAll(new ClientExitPacket(client.PlayerName).GetBytesTotal());
		}

		private Task ClientReadyHandler(ClientReadyPacket packet, IClient client, ref bool transitPackage) {
			transitPackage = false;
			return _slots.ClientSetReady(client, packet.SlotId, packet.Ready);
			
		}

		public GameRoomState(IClientSendingGroup sendingGroup, LobbyCreateRoomRequestPacket.RoomDescription description) {
			_sendingGroup = sendingGroup;
			_description = description;
			_slots = new RoomSlots(OnSlotChangedToAll, OnChangeReady);
		}

		private Task OnChangeReady(int slotId, bool isReady) {
			return _sendingGroup.SendAll(new ClientReadyPacket(slotId, isReady).GetBytesTotal());
		}

		private Task OnSlotChangedToAll(int slotId, IClient client, int colorIndex, int subColorIndex) {
			return OnSlotChanged(slotId, client, colorIndex, subColorIndex, _sendingGroup.SendAll);
		}



		private Task OnSlotChanged(int slotId, IClient client, int colorIndex, int subColorIndex, Func<byte[],Task> f) {
			var clientId = -1;
			var playerName = String.Empty;
			if (client != null) {
				playerName = client.PlayerName;
				clientId = client.ID;
			}
			return f(new ClientSelectedSlotPacket(clientId, playerName, slotId, colorIndex, subColorIndex).GetBytesTotal());
		}

		Task AsyncAddUnit(CreateUnitPacket packet, IClient client, ref bool transitPackage) {
			if (packet.OwnerId != _slots.ClientSlot(client)) {
				Console.WriteLine($"Not create unit id {packet.Id} because packet owner {packet.OwnerId} is not client.ID {client.ID}");
				transitPackage = false;
				return Task.CompletedTask;
			}
			Console.WriteLine($"Create Unit id {packet.Id} from owner {packet.OwnerId}");
			transitPackage = true;
			return _addUnitSemaphore.Lock(() => {
				var unit = new Unit() {
					SavedInitPacket = packet
				};
				_units[unit.SavedInitPacket.Id] = unit;
			});
		}
		
		void TopDownUnitMoveHandler(UnitTopDownPositionPacket packet, IClient client, ref bool transitPackage) {
			//Тут блокировка не нужна, используем потокобезапасный словарь
			if (_units.TryGetValue(packet.Id, out Unit unit)) {
				if (unit.SavedInitPacket.OwnerId != _slots.ClientSlot(client)) {
					Console.WriteLine($"Not move unit (id= {packet.Id}) because unit OwnerId= {unit.SavedInitPacket.OwnerId} is not client.ID={client.ID}");
					transitPackage = false;
					return;
				}
				unit.SavedInitPacket = new CreateUnitPacket() {
					Id = unit.SavedInitPacket.Id,
					OwnerId = unit.SavedInitPacket.OwnerId,
					Position = new Vector3(packet.XCoordinate , unit.SavedInitPacket.Position.y, packet.ZCoordinate),
					UnitTypeId = unit.SavedInitPacket.UnitTypeId,
					YAngle = packet.YAngle
				};
				transitPackage = true;
			}
		}

		public class Unit {
			public CreateUnitPacket SavedInitPacket;

		}

		public async Task OnNewClient(IClient newClient) {
			await newClient.Send(new EnterGameRoomResponcePacket(requestId: -1, success: true).GetBytesTotal());
			await newClient.Send(new GameInitPacket(_description, newClient.ID).GetBytesTotal());
			await _sendingGroup.SendAllFrom(newClient, new NewClientPacket(newClient.PlayerName).GetBytesTotal());
			await _slots.GetAllStateCallback(OnSlotChangedToRecipient, OnChangeReadyToRecipient);
			Task OnSlotChangedToRecipient(int slotId, IClient client, int colorIndex, int subClolorIndex) => 
				OnSlotChanged(slotId, client, colorIndex, subClolorIndex, newClient.Send);
			Task OnChangeReadyToRecipient(int slotId, bool isReady) => newClient.Send(new ClientReadyPacket(slotId, isReady).GetBytesTotal());
			if (_gameStarted) {
				await newClient.Send(new StartPlayPacket().GetBytesTotal());
			}
			await _addUnitSemaphore.Lock(async () => {
				var list = _units.ToList();
				foreach (var unit in list) {
					await newClient.Send(unit.Value.SavedInitPacket.GetBytesTotal());
				}
			});
		}
	}
}
