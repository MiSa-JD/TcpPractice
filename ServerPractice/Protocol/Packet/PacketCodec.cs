using System.Buffers.Binary;

namespace Protocol.Packet;

public class PacketCodec
{
  private PacketCodec () { }
  
  public const int headerSize = 6;
  private static PacketCodec? _manager;

  public static PacketCodec GetManager() { return _manager ??= new PacketCodec(); }

  // Byte 스트림용 배열 => PacketInfo
  public PacketInfo Bytes2Packet(byte[] buffer)
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
  public PacketInfo Data2Packet(short type, byte[] payload)
  {
    return new PacketInfo(type, payload);
  }

  public PacketInfo Data2Packet(PacketType type, byte[] payload)
  {
    return Data2Packet((short)type, payload);
  }
  
  // PacketInfo => Byte 스트림용 배열
  public byte[] Packet2Bytes(PacketInfo packet)
  {
    byte[] buffer = new byte[headerSize + packet.payload.Length];
    
    BinaryPrimitives.WriteInt32BigEndian(buffer, packet.payload.Length);
    BinaryPrimitives.WriteInt16BigEndian(buffer, (short)packet.type);
    packet.payload.CopyTo(buffer.AsSpan(headerSize));

    return buffer;
  }
}