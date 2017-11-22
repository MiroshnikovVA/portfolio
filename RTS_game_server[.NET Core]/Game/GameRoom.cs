using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using SpaceCrabDevelopment.Network;
using System.IO;
using ConsoleMars2000Server.Abstractions;
using ConsoleMars2000Server.Implementation;

namespace ConsoleMars2000Server.Game
{
	public class GameRoom : BaseRoom {

		private GameRoomState _roomState;
		IHostRoom _parentRoom;
		private int _id;
		private LobbyCreateRoomRequestPacket.RoomDescription _description;

		protected override IRoomState RoomState => _roomState;

		public override int Id => _id;

		public GameRoom(IHostRoom parentRoom, int id, LobbyCreateRoomRequestPacket.RoomDescription description) {
			_parentRoom = parentRoom;
			_id = id;
			_description = description;
			_roomState = new GameRoomState(ClientSendingGroup, _description);
		}

		protected override void OnZeroCount() {
			ClientSendingGroup.Close();
			_parentRoom.OnChildrenRoomClosed(this as IRoom);
		}

		public override async Task<bool> TryExit(IClient client) {
			await ClientSendingGroup.RemoveClient(client);
			await _roomState.RemoveClient(client);
			if (_parentRoom != null)
				return await _parentRoom.TryEnter(client);
			return false;
		}

		protected override void RemoveClient(IClient client) {
			var noawait = _roomState.RemoveClient(client);
		}
	}
}
