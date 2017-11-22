using System;
using System.IO;
using UnityEngine;

namespace SpaceCrabDevelopment.Network {

	public interface IPacket {
		void Write(BinaryWriter wr);
	}

	public class PacketAttribute : Attribute {
		public byte Tag { get; set; }
	}

	public static class PacketHelper {
		public static byte[] GetBytes(this IPacket packet) {
			using (var ms = new MemoryStream()) {
				using (var wr = new BinaryWriter(ms)) {
					packet.Write(wr);
					return ms.ToArray();
				}
			}
		}

		public static byte[] GetBytesTotal(this IPacket packet) {
			using (var ms = new MemoryStream()) {
				using (var wr = new BinaryWriter(ms)) {
					wr.Write((int)0);
					packet.Write(wr);
					ms.Position = 0;
					wr.Write(((int)ms.Length) - 4);
					return ms.ToArray();
				}
			}
		}
	}
}
