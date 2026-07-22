using System.Text;

namespace Protocol.Packet;

public static class PacketEncoder
{
  // 100
  public static PacketInfo EncodeBroadcastRequest(string message)
  {
    byte[] payload = Encoding.Unicode.GetBytes(message);
    return PacketCodec
      .GetManager()
      .Data2Packet((short)PacketType.BroadcastRequest, payload);
  }

  // 200
  public static PacketInfo EncodeSystemMessage(char level, string message)
  {
    int length = Encoding.Unicode.GetByteCount(message);
    byte[] payload = new byte[sizeof(char) + length];
    payload[0] = (byte)level;
    Encoding.Unicode
      .GetBytes(message)
      .CopyTo(payload.AsSpan(1));
    
    return PacketCodec
      .GetManager()
      .Data2Packet((short)PacketType.SystemMessage, payload);
  }
}