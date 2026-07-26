using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record ApplyEntranceResponse(int count, List<RoomUserOnPacket> users) : IPayload
{
  public PacketInfo ToPacket()
  {
    byte[] countBytes = new byte[4];
    BinaryPrimitives.WriteInt32BigEndian(countBytes, this.count);
    byte[] userBytes = [];
    foreach (var user in users)
      userBytes = userBytes.Concat(user.Turn2Bytes()).ToArray();

    byte[] payload = countBytes
      .Concat(userBytes)
      .ToArray();

    return new PacketInfo(
      PacketType.ApplyEntranceResponse,
      payload);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int userCount = BinaryPrimitives.ReadInt32BigEndian(bytes);
    var users = new List<RoomUserOnPacket>();

    for (int i = 4; i < bytes.Length;)
    {
      int userRoomId = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(i, 4));
      i += 4;
      int nameLength = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(i, 4));
      i += 4;
      string name = Encoding.UTF8.GetString(bytes.AsSpan(i, nameLength));
      i += nameLength;
      users.Add(new RoomUserOnPacket(userRoomId, name));
    }
    
    payload = new ApplyEntranceResponse(userCount, users);
  }
}