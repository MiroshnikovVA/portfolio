using ConsoleMars2000Server.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Implementation {
    public abstract class BaseRoom: IRoom {

		protected IClientSendingGroup ClientSendingGroup;
        ClientReader _clientReader;

        //public abstract int Id { get; }

        public BaseRoom() {
            _clientReader = new ClientReader(this);
			ClientSendingGroup = new ClientSendingGroup();
			ClientSendingGroup.OnNewClient += _clientContainer_OnNewClient;
			ClientSendingGroup.OnZero += _clientContainer_OnZero;
		}

		async Task _clientContainer_OnNewClient(IClient client) {
			await RoomState.OnNewClient(client);
		}

		void _clientContainer_OnZero() {
			OnZeroCount();
		}

		protected abstract void RemoveClient(IClient client);

		class ClientReader : IClientReader {

			BaseRoom _baseRoom;

			public ClientReader(BaseRoom baseRoom) {
				_baseRoom = baseRoom;
			}

			void IClientReader.OnClientDisconnect(IClient client) {
				var noWait = _baseRoom.ClientSendingGroup.RemoveClient(client);
				_baseRoom.RemoveClient(client);
			}

			Task IClientReader.OnClientRecivePacket(IClient client, byte[] packet, int totalPacketSize, MemoryStream stream, BinaryReader reader) {
				return _baseRoom.RoomState.AddPacket(client, packet, totalPacketSize, stream, reader);
			}
		}

        async Task<bool> IRoom.TryEnter(IClient client) {
			//Транзакция откладываем отправку сообщений данному клиенту в очередь, до тех пор, пока транзакция не завершится
			//Таким образом клиент не узнает что он в новой комнате раньше времени, и не пошлет ответные сообщения, к которым сервер еще не готов
			var rez = false;
			try {
				client.BeginChangingRoomTransaction();
				if (await ClientSendingGroup.TryAddClient(client)) {
					client.ChangeRoom(_clientReader);
					rez = true;
				}
			}
			finally {
				client.FinallyChangingRoomTransaction();
			}
			return rez;
        }

        #region Abstract

        public abstract Task<bool> TryExit(IClient clientState);

		protected abstract void OnZeroCount();

		protected abstract IRoomState RoomState { get; }

		public abstract int Id { get; }

		#endregion

	}
}
