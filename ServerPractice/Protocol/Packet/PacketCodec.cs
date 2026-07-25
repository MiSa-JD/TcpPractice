using System.Buffers.Binary;
using Protocol.Packet.Models;

namespace Protocol.Packet;

public static class PacketCodec
{
  public const int headerSize = 6;

  // Byte 스트림용 배열 => PacketInfo
  public static PacketInfo Bytes2Packet(byte[] buffer)
  {
    if (buffer.Length < headerSize)
      throw new Exception("Not enough header bytes.");
    
    int length = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(0,4));
    
    if (length < 0)
      throw new Exception("Wrong packet lenght.");

    if (buffer.Length < 4 + 2 + length)
      throw new Exception("Packet buffer is too small.");

    return new PacketInfo(
      type: BinaryPrimitives.ReadInt16BigEndian(buffer.AsSpan(4,2)),
      payload: buffer.AsSpan(headerSize).ToArray());
  }
  
  // 패킷 내용 => PacketInfo
  public static PacketInfo Data2Packet(PacketType type)
  {
    return Data2Packet(type, []);
  }

  public static PacketInfo Data2Packet(PacketType type, byte[] payload)
  {
    return new PacketInfo(type, payload);
  }
  
  // PacketInfo => Byte 스트림용 배열
  public static byte[] Packet2Bytes(PacketInfo packet)
  {
    byte[] buffer = new byte[headerSize + packet.payload.Length];
    
    BinaryPrimitives.WriteInt32BigEndian(buffer, packet.payload.Length);
    BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(4), (short)packet.type);
    packet.payload.CopyTo(buffer.AsSpan(headerSize));

    return buffer;
  }
}