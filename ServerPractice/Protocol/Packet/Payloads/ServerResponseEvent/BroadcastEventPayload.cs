using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record BroadcastEventPayload(int senderId, string message) : IPayload
{
  public BroadcastEventPayload(int senderId, byte[] messagePayload)
    : this(senderId, Encoding.Unicode.GetString(messagePayload)) { }
  public PacketInfo ToPacket()
  {
    int messageLength = Encoding.Unicode.GetByteCount(message);
    
    byte[] packet = new byte[4 + 4 + messageLength];
    BinaryPrimitives.WriteInt32BigEndian(packet, senderId);
    
    BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(4), messageLength);
    Encoding.Unicode.GetBytes(message).CopyTo(packet.AsSpan(4+4));
    
    return new PacketInfo(
      PacketType.BroadcastEvent,
      packet);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int senderId = BinaryPrimitives.ReadInt32BigEndian(bytes);
    int messageLength = BinaryPrimitives.ReadInt32BigEndian(
      bytes.AsSpan(4));
    string message = Encoding.Unicode.GetString(
      bytes.AsSpan(4 + 4));
    
    payload = new BroadcastEventPayload(senderId, message);
  }
}