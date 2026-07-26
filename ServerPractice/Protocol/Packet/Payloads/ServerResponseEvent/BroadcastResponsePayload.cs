using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record BroadcastResponsePayload(string username, string message) : IPayload
{
  public BroadcastResponsePayload(string username, byte[] messagePayload)
    : this(username, Encoding.Unicode.GetString(messagePayload)) { }
  public PacketInfo ToPacket()
  {
    int nameLength = Encoding.UTF8.GetByteCount(username);
    int messageLength = Encoding.Unicode.GetByteCount(message);
    
    byte[] packet = new byte[4 + nameLength + 4 + messageLength];
    BinaryPrimitives.WriteInt32BigEndian(packet, nameLength);
    Encoding.UTF8.GetBytes(username).CopyTo(packet.AsSpan(4, nameLength));
    
    BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(4+nameLength, messageLength), messageLength);
    Encoding.Unicode.GetBytes(message).CopyTo(packet.AsSpan(4+nameLength+4));
    
    return new PacketInfo(
      PacketType.BroadcastEvent,
      packet);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int nameLength = BinaryPrimitives.ReadInt32BigEndian(
      bytes.AsSpan(0, 4));
    string name = Encoding.UTF8.GetString(
      bytes.AsSpan(4, nameLength));
    int messageLength = BinaryPrimitives.ReadInt32BigEndian(
      bytes.AsSpan(4 + nameLength));
    string message = Encoding.Unicode.GetString(
      bytes.AsSpan(4 + nameLength + 4));
    
    payload = new BroadcastResponsePayload(name, message);
  }
}