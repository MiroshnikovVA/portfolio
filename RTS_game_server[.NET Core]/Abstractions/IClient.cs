using System;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Abstractions {
	public interface IClient {
		int ID { get; }
		Task Send(byte[] sendMessage);
		Task Send(ArraySegment<byte> sendMessage);

		string PlayerName { get; }

		///// <summary> потокобезопасно откладывает в очередь отправку сообщений данному клиенту до тех пор, пока транзакция не завершится</summary>
		//Task Transaction(Func<Task> action);

		void BeginChangingRoomTransaction();

		void FinallyChangingRoomTransaction();

		/// <summary> Осуществляет смену парсера, и клиент теперь может снова получать сообщения из новой комнаты
		/// сответственно клиент получит пакет инициализаии в новой комнате и узнает об этом
		/// </summary>
		void ChangeRoom(IClientReader newClientReader);
    }
}
