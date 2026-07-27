using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record ExitedUserInfoEventPayload(int roomUserId, string username): IPayload
{
  public PacketInfo ToPacket()
  {
    int nameLength = Encoding.UTF8.GetByteCount(username);
    byte[] buffer = new byte[4 + 4 + nameLength];
    
    BinaryPrimitives.WriteInt32BigEndian(buffer, roomUserId);
    BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(4), nameLength);
    Encoding.UTF8.GetBytes(username).CopyTo(buffer.AsSpan(4+4));

    return new PacketInfo(
      PacketType.ExitedUserInfoEvent,
      buffer);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int roomUserId = BinaryPrimitives.ReadInt32BigEndian(bytes);
    string name = Encoding.UTF8.GetString(bytes.AsSpan(4 + 4));
    payload = new ExitedUserInfoEventPayload(roomUserId, name);
  }
}