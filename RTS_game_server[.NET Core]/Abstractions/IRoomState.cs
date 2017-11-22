using System.IO;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Abstractions {
    public interface IRoomState
    {
		Task AddPacket(IClient clientState, byte[] packet, int totalPacketSize, MemoryStream stream, BinaryReader reader);

		Task OnNewClient(IClient client);
	}
}
