using ConsoleMars2000Server.Implementation;
using System;
using System.Threading.Tasks;

namespace ConsoleMars2000Server {
	public class Program {

		public static void Main(string[] args) {
			int tcpPort = 63217;
			int udpPort = 63217;

			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("Server starting at tcp port ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(tcpPort);
			Console.ResetColor();

			var server = new Server();
			server.AsyncStart(udpPort, tcpPort).Wait();
			//System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
		}
	}
}
