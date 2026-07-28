using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public record UnicastRequestPayload(int receiverId, string message) : IPayload
{
  public PacketInfo ToPacket()
  {
    int messageLength = Encoding.Unicode.GetByteCount(message);

    byte[] buffer = new byte[4+4+messageLength];
    BinaryPrimitives.WriteInt32BigEndian(buffer, receiverId);
    BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(4), messageLength);
    Encoding.Unicode.GetBytes(message).CopyTo(buffer.AsSpan(4+4));

    return new PacketInfo(
      PacketType.UnicastRequest,
      buffer);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int roomUserId = BinaryPrimitives.ReadInt32BigEndian(bytes);
    string message = Encoding.Unicode.GetString(bytes.AsSpan(4+4));
    
    payload = new UnicastRequestPayload(roomUserId, message);
  }
}