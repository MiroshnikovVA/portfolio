using ConsoleMars2000Server.Abstractions;
using ConsoleMars2000Server.Lobby;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Implementation {

	public class Server {

		public Task AsyncStart(int udpPort, int tcpPort) {
			List<Task> tasks = new List<Task>();
			//var udpTask = StartSocketServer(udpPort, SocketType.Dgram, ProtocolType.Udp);
			var tcpTask = StartSocketServer(udpPort, SocketType.Stream, ProtocolType.Tcp);
			tasks.Add(tcpTask);
			return Task.WhenAll(tasks);
		}

		Task StartSocketServer(int port, SocketType socketType, ProtocolType protocolType) {
			var socket = new Socket(socketType, protocolType);
			socket.Bind(new IPEndPoint(IPAddress.Any, port));
			socket.Listen(1000);
			return Listener(socket);
		}

		volatile int _nextClientId = 0;

		public async Task Listener(Socket socket) {
			for (;;) {
				var client = await socket.AcceptAsync();
				var clientTask = ClientWork(client);
			}
		}

		public IRoom _defaultRoom = new LobbyRoom();

		public async Task ClientWork(Socket client) {
			var id = System.Threading.Interlocked.Increment(ref _nextClientId);
			Client clientState = new Client(client, id);
			client.NoDelay = true;
			var task = _defaultRoom.TryEnter(clientState);
			await clientState.TCPSocketWork();
		}



	}
}
