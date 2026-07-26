using System.Text;
using Protocol.Packet.Models;
using Protocol.SystemMessage;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record SystemMessagePayload(SystemMessageType level, string message) : IPayload
{
  public PacketInfo ToPacket()
  {
    int length = Encoding.Unicode.GetByteCount(message);
    byte[] payload = new byte[1 + length];
    payload[0] = (byte)level;
    Encoding.Unicode
      .GetBytes(message)
      .CopyTo(payload.AsSpan(1));
    
    return new PacketInfo(
      PacketType.SystemMessageEvent,
      payload);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    var type = (SystemMessageType)bytes[0];
    var msg = Encoding.Unicode.GetString(bytes.AsSpan(1));
    payload = new SystemMessagePayload(type, msg);
  }
}