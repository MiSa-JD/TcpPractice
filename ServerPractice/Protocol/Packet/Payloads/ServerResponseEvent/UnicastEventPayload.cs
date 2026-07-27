using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record UnicastEventPayload(string senderName, string message): IPayload
{
  public UnicastEventPayload(string username, byte[] messagePayload)
    : this(username, Encoding.Unicode.GetString(messagePayload)) { }
  public PacketInfo ToPacket()
  {
    int nameLength = Encoding.UTF8.GetByteCount(senderName);
    int messageLength = Encoding.Unicode.GetByteCount(message);
    
    byte[] buffer = new byte[4+nameLength+4+messageLength];
    BinaryPrimitives.WriteInt32BigEndian(buffer, nameLength);
    Encoding.UTF8.GetBytes(senderName).CopyTo(buffer.AsSpan(4));
    BinaryPrimitives.WriteInt32BigEndian(
      buffer.AsSpan(4+nameLength), messageLength);
    Encoding.Unicode.GetBytes(message)
      .CopyTo(buffer.AsSpan(4+nameLength+4));

    return new PacketInfo(
      PacketType.UnicastEvent,
      buffer);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int nameLength = BinaryPrimitives.ReadInt32BigEndian(bytes);
    string name = Encoding.UTF8.GetString(bytes.AsSpan(4, nameLength));
    string message = Encoding.Unicode.GetString(bytes.AsSpan(4 + nameLength + 4));

    payload = new UnicastEventPayload(name, message);
  }
}