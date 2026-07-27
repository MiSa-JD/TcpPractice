using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record ApplyEntranceResponsePayload(List<RoomUserOnPacket> users) : IPayload
{
  public PacketInfo ToPacket()
  {
    byte[] countBytes = new byte[4];
    BinaryPrimitives.WriteInt32BigEndian(countBytes, users.Count);
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
    UserListResponsePayload.ReadBytes(bytes, out payload);
  }
}