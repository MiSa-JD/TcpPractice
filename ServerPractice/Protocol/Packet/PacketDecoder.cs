using System.Buffers.Binary;
using System.Text;

namespace Protocol.Packet;

public static class PacketDecoder
{
  // 100
  public static string DecodeBroadcastRequest(byte[] buffer)
  {
    int length = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(0,4));
    if (buffer.Length - PacketCodec.headerSize < length)
      throw new ArgumentOutOfRangeException();
    return Encoding.Unicode.GetString(
      buffer.AsSpan(
        PacketCodec.headerSize, 
        length));
  }
}