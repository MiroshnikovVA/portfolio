using System;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Abstractions {
	public interface IClientSendingGroup {

		Task SendAllFrom(IClient from, byte[] packet);

        Task SendAll(byte[] packet);

        Task SendAllFrom(IClient from, byte[] packet, int totalPacketSize);

		/// <summary> вернет false, если комната уже закрыта </summary>
		Task<bool> TryAddClient(IClient clientState);

		Task RemoveClient(IClient clientState);

		event Action OnZero;

		event Func<IClient, Task> OnNewClient;

		void Close();
	}
}
