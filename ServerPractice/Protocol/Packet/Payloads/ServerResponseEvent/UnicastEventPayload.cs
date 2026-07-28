using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record UnicastEventPayload(int senderId, string message): IPayload
{
  public UnicastEventPayload(int senderId, byte[] messagePayload)
    : this(senderId, Encoding.Unicode.GetString(messagePayload)) { }
  public PacketInfo ToPacket()
  {
    int messageLength = Encoding.Unicode.GetByteCount(message);
    
    byte[] buffer = new byte[4+4+messageLength];
    BinaryPrimitives.WriteInt32BigEndian(buffer, senderId);
    BinaryPrimitives.WriteInt32BigEndian(
      buffer.AsSpan(4), messageLength);
    Encoding.Unicode.GetBytes(message)
      .CopyTo(buffer.AsSpan(4+4));

    return new PacketInfo(
      PacketType.UnicastEvent,
      buffer);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int senderId = BinaryPrimitives.ReadInt32BigEndian(bytes);
    string message = Encoding.Unicode.GetString(bytes.AsSpan(4 + 4));

    payload = new UnicastEventPayload(senderId, message);
  }
}