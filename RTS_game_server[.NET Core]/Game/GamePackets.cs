using System.IO;
using UnityEngine;

namespace SpaceCrabDevelopment.Network {
	[Packet(Tag = Tag)]
	public struct CreateUnitPacket : IPacket {
		public CreateUnitPacket(int unitTypeId, int id, int ownerId, Vector3 position, float yAngle) {
			UnitTypeId = unitTypeId;
			Id = id;
			OwnerId = ownerId;
			Position = position;
			YAngle = yAngle;
		}

		public CreateUnitPacket(BinaryReader reader) {
			UnitTypeId = reader.ReadInt32();
			Id = reader.ReadInt32();
			OwnerId = reader.ReadInt32();
			Position = new Vector3() {
				x = reader.ReadSingle(),
				y = reader.ReadSingle(),
				z = reader.ReadSingle()
			};
			YAngle = reader.ReadSingle();
		}

		public int UnitTypeId;
		public int Id;
		public int OwnerId;
		public Vector3 Position;
		public float YAngle;

		public const byte Tag = (byte)GamePackets.UnitCreate;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(UnitTypeId);
			wr.Write(Id);
			wr.Write(OwnerId);
			wr.Write(Position.x);
			wr.Write(Position.y);
			wr.Write(Position.z);
			wr.Write(YAngle);
		}
	}

	[Packet(Tag = Tag)]
	public struct UnitTopDownPositionPacket : IPacket {
		public UnitTopDownPositionPacket(int id, float xCoordinate, float zCoordinate, float yAngle) {
			Id = id;
			XCoordinate = xCoordinate;
			ZCoordinate = zCoordinate;
			YAngle = yAngle;
		}

		public UnitTopDownPositionPacket(BinaryReader reader) {
			Id = reader.ReadInt32();
			XCoordinate = reader.ReadSingle();
			ZCoordinate = reader.ReadSingle();
			YAngle = reader.ReadSingle();
		}

		public int Id;
		public float XCoordinate;
		public float ZCoordinate;
		public float YAngle;

		public const byte Tag = (byte)GamePackets.TopDownUnitMove;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(Id);
			wr.Write(XCoordinate);
			wr.Write(ZCoordinate);
			wr.Write(YAngle);
		}
	}

	[Packet(Tag = Tag)]
	public struct GameInitPacket : IPacket {
		public GameInitPacket(LobbyCreateRoomRequestPacket.RoomDescription description, int clientId) {
			Description = description;
			ClientId = clientId;
		}

		public GameInitPacket(BinaryReader reader) {
			Description = new LobbyCreateRoomRequestPacket.RoomDescription(reader);
			ClientId = reader.ReadInt32();
		}

		public LobbyCreateRoomRequestPacket.RoomDescription Description;
		public int ClientId;

		public const byte Tag = (byte)GamePackets.GameInit;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			Description.Write(wr);
			wr.Write(ClientId);
		}
	}

	[Packet(Tag = Tag)]
	public struct NewClientPacket : IPacket {
		public NewClientPacket(string playerName) {
			PlayerName = playerName;
		}

		public NewClientPacket(BinaryReader reader) {
			PlayerName = reader.ReadString();
		}

		public string PlayerName;

		public const byte Tag = (byte)GamePackets.NewClient;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(PlayerName);
		}
	}

	[Packet(Tag = Tag)]
	public struct ClientExitPacket : IPacket {
		public ClientExitPacket(string playerName) {
			PlayerName = playerName;
		}

		public ClientExitPacket(BinaryReader reader) {
			PlayerName = reader.ReadString();
		}

		public string PlayerName;

		public const byte Tag = (byte)GamePackets.ClientExit;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(PlayerName);
		}
	}

	[Packet(Tag = Tag)]
	public struct TrySelectSlotRequestPacket : IPacket {
		public TrySelectSlotRequestPacket(int slotId) {
			SlotId = slotId;
		}

		public TrySelectSlotRequestPacket(BinaryReader reader) {
			SlotId = reader.ReadInt32();
		}

		public int SlotId;

		public const byte Tag = (byte)GamePackets.SelectSlotRequestPacket;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(SlotId);
		}
	}

	[Packet(Tag = Tag)]
	public struct ClientReadyPacket : IPacket {
		public ClientReadyPacket(int slotId, bool ready) {
			SlotId = slotId;
			Ready = ready;
		}

		public ClientReadyPacket(BinaryReader reader) {
			SlotId = reader.ReadInt32();
			Ready = reader.ReadBoolean();
		}

		public int SlotId;
		public bool Ready;

		public const byte Tag = (byte)GamePackets.ClientReady;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(SlotId);
			wr.Write(Ready);
		}
	}

	[Packet(Tag = Tag)]
	public struct StartPlayPacket : IPacket {

		public const byte Tag = (byte)GamePackets.StartPlay;

		public StartPlayPacket(BinaryReader reader) {
		}

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
		}
	}

	[Packet(Tag = Tag)]
	public struct ClientSelectedSlotPacket : IPacket {
		public ClientSelectedSlotPacket(int clientId, string name, int slotId, int colorIndex, int subColorIndex) {
			ClientId = clientId;
			Name = name;
			SlotId = slotId;
			ColorIndex = colorIndex;
			SubColorIndex = subColorIndex;
		}

		public ClientSelectedSlotPacket(BinaryReader reader) {
			ClientId = reader.ReadInt32();
			Name = reader.ReadString();
			SlotId = reader.ReadInt32();
			ColorIndex = reader.ReadInt32();
			SubColorIndex = reader.ReadInt32();
		}

		public int ClientId;
		public string Name;
		public int SlotId;
		public int ColorIndex;
		public int SubColorIndex;

		public const byte Tag = (byte)GamePackets.SelectSlotRequestPacket;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(ClientId);
			wr.Write(Name);
			wr.Write(SlotId);
			wr.Write(ColorIndex);
			wr.Write(SubColorIndex);
		}
	}

	[Packet(Tag = Tag)]
	public struct ChangeColorPacket : IPacket {
		public ChangeColorPacket(int slotId, int colorIndex, int subColorIndex) {
			SlotId = slotId;
			ColorIndex = colorIndex;
			SubColorIndex = subColorIndex;
		}

		public ChangeColorPacket(BinaryReader reader) {
			SlotId = reader.ReadInt32();
			ColorIndex = reader.ReadInt32();
			SubColorIndex = reader.ReadInt32();
		}

		public int SlotId;
		public int ColorIndex;
		public int SubColorIndex;

		public const byte Tag = (byte)GamePackets.ChangeColor;

		public void Write(BinaryWriter wr) {
			wr.Write(Tag);
			wr.Write(SlotId);
			wr.Write(ColorIndex);
			wr.Write(SubColorIndex);
		}
	}

	public enum GamePackets : byte {
		TopDownUnitMove = 0,
		UnitCreate = 1,
		GameInit = 2,
		NewClient = 3,
		ClientReady = 4,
		ClientExit = 5,
		SelectSlotRequestPacket = 6,
		ClientSelectedSlot = 7,
		StartPlay = 8,
		ChangeColor = 9,
	}
}
