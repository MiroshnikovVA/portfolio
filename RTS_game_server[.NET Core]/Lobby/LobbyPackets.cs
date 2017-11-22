using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpaceCrabDevelopment.Network {

	[Packet(Tag = Tag)]
	public struct LobbyInitPacket : IPacket {
		public LobbyInitPacket(int clientId) {
			ClientId = clientId;
		}

		public LobbyInitPacket(BinaryReader reader) {
			ClientId = reader.ReadInt32();
		}

		public int ClientId;

		public const byte Tag = (byte)LobbyPackets.LobbyInit;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(ClientId);
		}
	}
	[Packet(Tag = Tag)]
	public struct LobbyCreateRoomRequestPacket : IPacket {

		public struct RoomDescription {
			public int PlayersCount;

			public RoomDescription(BinaryReader reader) {
				PlayersCount = reader.ReadInt32();
			}

			public RoomDescription(int playersCount) {
				PlayersCount = playersCount;
			}

			public void Write(BinaryWriter wr) {
				wr.Write(PlayersCount);
			}
		}

		public LobbyCreateRoomRequestPacket(int requestId, string roomName, RoomDescription description) {
			RequestId = requestId;
			RoomName = roomName;
			Description = description;
		}

		public LobbyCreateRoomRequestPacket(BinaryReader reader) {
			RequestId = reader.ReadInt32();
			RoomName = reader.ReadString();
			Description = new RoomDescription(reader);
		}

		public int RequestId;
		public string RoomName;
		public RoomDescription Description;

		public const byte Tag = (byte)LobbyPackets.LobbyCreateRoomRequest;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(RequestId);
			wr.Write(RoomName);
			Description.Write(wr);
		}
	}
	[Packet(Tag = Tag)]
	public struct LobbyEnterRoomRequestPacket : IPacket {
		public LobbyEnterRoomRequestPacket(int requestId, int roomId) {
			RequestId = requestId;
			RoomId = roomId;
		}

		public LobbyEnterRoomRequestPacket(BinaryReader reader) {
			RequestId = reader.ReadInt32();
			RoomId = reader.ReadInt32();
		}

		public int RequestId;
		public int RoomId;

		public const byte Tag = (byte)LobbyPackets.LobbyEnterRoomRequest;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(RequestId);
			wr.Write(RoomId);
		}
	}
	[Packet(Tag = Tag)]
	public struct EnterGameRoomResponcePacket : IPacket {
		public EnterGameRoomResponcePacket(int requestId, bool success) {
			RequestId = requestId;
			Success = success;
		}

		public EnterGameRoomResponcePacket(BinaryReader reader) {
			RequestId = reader.ReadInt32();
			Success = reader.ReadBoolean();
		}

		public int RequestId;
		public bool Success;

		public const byte Tag = (byte)LobbyPackets.EnterRoomResponce;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(RequestId);
			wr.Write(Success);
		}
	}
	[Packet(Tag = Tag)]
	public struct LobbyRoomsListRequestPacket : IPacket {

		public LobbyRoomsListRequestPacket(BinaryReader reader) {
		}

		public const byte Tag = (byte)LobbyPackets.LobbyRoomsList;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
		}
	}
	[Packet(Tag = Tag)]
	public struct LobbyRoomsListResponcePacket : IPacket {

		public List<RoomRecord> Rooms;

		public LobbyRoomsListResponcePacket(List<RoomRecord> rooms) {
			Rooms = rooms;
		}

		public LobbyRoomsListResponcePacket(BinaryReader reader) {
			var count = reader.ReadInt32();
			Rooms = new List<RoomRecord>();
			for (int i = 0; i < count; i++) {
				var record = new RoomRecord();
				RoomRecordRead(record, reader);
				Rooms.Add(record);
			}
		}

		void RoomRecordRead(RoomRecord room, BinaryReader reader) {
			room.Name = reader.ReadString();
			room.Id = reader.ReadInt32();
		}

		void RoomRecordWrite(RoomRecord room, BinaryWriter writer) {
			writer.Write(room.Name);
			writer.Write(room.Id);
		}

		[System.Serializable]
		public class RoomRecord {
			public string Name;
			public int Id;
		}

		public const byte Tag = (byte)LobbyPackets.LobbyRoomsList;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(Rooms.Count);
			for (int i = 0; i < Rooms.Count; i++) {
				var reord = Rooms[i];
				RoomRecordWrite(reord, wr);
			}
		}
	}

	public enum LobbyPackets : byte {
		LobbyInit = 100,
		LobbyCreateRoomRequest = 101,
		LobbyEnterRoomRequest = 102,
		EnterRoomResponce = 103,
		LobbyRoomsList = 104
	}
}
