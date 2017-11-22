using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Abstractions { 
	public interface IClientReader {
		Task OnClientRecivePacket(IClient clientState, byte[] packet, int totalPacketSize, MemoryStream stream, BinaryReader reader);

		void OnClientDisconnect(IClient clientState);
	}
}
