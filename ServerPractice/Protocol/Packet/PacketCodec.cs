using System.Buffers.Binary;
using Protocol.Packet.Models;

namespace Protocol.Packet;

public static class PacketCodec
{
  private const int headerSize = 7;
  public const byte MagicByte = 30;

  // Byte 스트림용 배열 => PacketInfo
  // 패킷 받을 때 전체 패킷의 길이를 계산할 수 없어서 각자가 첫 체크 -> 길이 체크 구조를 띄고 있어서 이거 안 쓰게 되는 듯
  // 남겨야하나
  public static PacketInfo Bytes2Packet(byte[] buffer)
  {
    if (buffer[0] != MagicByte)
      throw new Exception("Wrong packet received.");
    
    if (buffer.Length < headerSize)
      throw new Exception("Not enough header bytes.");
    
    int length = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(1,4));
    
    if (length < 0)
      throw new Exception("Wrong packet lenght.");

    if (buffer.Length < headerSize + length)
      throw new Exception("Packet buffer is too small.");

    return new PacketInfo(
      type: BinaryPrimitives.ReadInt16BigEndian(buffer.AsSpan(5,2)),
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
    buffer[0] = MagicByte;
    BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), packet.payload.Length);
    BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(5), (short)packet.type);
    packet.payload.CopyTo(buffer.AsSpan(headerSize));

    return buffer;
  }
}