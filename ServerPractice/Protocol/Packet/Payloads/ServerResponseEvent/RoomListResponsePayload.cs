using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record RoomListResponsePayload(int roomCount, List<RoomOnPacket> rooms): IPayload
{
  public PacketInfo ToPacket()
  {
    byte[] roomCountBytes = new byte[4];
    BinaryPrimitives.WriteInt32BigEndian(roomCountBytes, roomCount);
    
    byte[] roomsBytes = [];
    foreach (var room in rooms)
      roomsBytes = roomsBytes.Concat(room.Turn2Bytes()).ToArray();
    
    byte[] payload = roomCountBytes
      .Concat(roomsBytes)
      .ToArray();

    return new PacketInfo(
      PacketType.RoomListResponse,
      payload);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    var rooms = new List<RoomOnPacket>();
    
    for (int i = 4; i < bytes.Length;)
    {
      int roomId = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(i, 4));
      i += 4;
      int titleLength = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(i, 4));
      i += 4;
      string title = Encoding.Unicode.GetString(bytes.AsSpan(i, titleLength));
      i += titleLength;
      int curUserCount = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(i, 4));
      i += 4;
      rooms.Add(new RoomOnPacket(roomId, title, curUserCount));
    }
    
    payload = new RoomListResponsePayload(rooms.Count, rooms);
  }
}