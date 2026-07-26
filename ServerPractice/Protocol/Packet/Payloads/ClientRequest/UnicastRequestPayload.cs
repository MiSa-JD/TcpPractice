using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public record UnicastRequestPayload(Guid receiverId, string message) : IPayload
{
  public PacketInfo ToPacket()
  {
    int messageLength = Encoding.Unicode.GetByteCount(message);

    byte[] buffer = new byte[16+4+messageLength];
    receiverId.ToByteArray().CopyTo(buffer, 0);
    BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(16), messageLength);
    Encoding.Unicode.GetBytes(message).CopyTo(buffer.AsSpan(16+4));

    return new PacketInfo(
      PacketType.UnicastRequest,
      buffer);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    Guid uuid = new Guid(bytes.AsSpan(0, 16));
    int messageLength = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
    string message = Encoding.Unicode.GetString(bytes.AsSpan(16 + 4));
    
    payload = new UnicastRequestPayload(uuid, message);
  }
}