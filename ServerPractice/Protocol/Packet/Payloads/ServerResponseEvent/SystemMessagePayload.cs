using System.Text;
using Protocol.Packet.Models;
using Protocol.SystemMessage;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record SystemMessagePayload(SystemMessageType level, string message) : IPayload
{
  public PacketInfo ToPacket()
  {
    int length = Encoding.Unicode.GetByteCount(message);
    byte[] payload = new byte[sizeof(char) + length];
    payload[0] = (byte)level;
    Encoding.Unicode
      .GetBytes(message)
      .CopyTo(payload.AsSpan(1));
    
    return PacketCodec
      .Data2Packet(PacketType.SystemMessageEvent, payload);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    var type = (SystemMessageType)bytes[0];
    var msg = Encoding.Unicode.GetString(bytes, 1, bytes.Length - 2);
    payload = new SystemMessagePayload(type, msg);
  }
}