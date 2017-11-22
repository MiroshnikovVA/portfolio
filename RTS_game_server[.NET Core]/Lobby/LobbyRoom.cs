using ConsoleMars2000Server.Abstractions;
using ConsoleMars2000Server.Implementation;
using SpaceCrabDevelopment.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleMars2000Server.Lobby {
	public class LobbyRoom : BaseRoom, IHostRoom {

		private LobbyRoomState _roomState;
		protected override IRoomState RoomState { get { return _roomState; } }

		public override int Id => -1;

		public LobbyRoom() {
			_roomState = new LobbyRoomState(ClientSendingGroup, this);
		}

		public void OnChildrenRoomClosed(IRoom childrenRoom) {
            _roomState.OnRemoveChildrenRoom(childrenRoom);
        }

		protected override void OnZeroCount() {	}

		public override async Task<bool> TryExit(IClient clientState) {
			await Task.CompletedTask;
			return false; //Пока что выходить из лобби нельзя
		}

		protected override void RemoveClient(IClient client) {
			//var noawait = _roomState.RemoveClient(client);
		}
	}
}
