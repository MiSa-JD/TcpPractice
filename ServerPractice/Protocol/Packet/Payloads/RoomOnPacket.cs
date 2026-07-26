using System.Buffers.Binary;
using System.Text;

namespace Protocol.Packet.Payloads;

public record RoomOnPacket(
  int roomId,
  string title,
  int curUserCount)
{
  public byte[] Turn2Bytes()
  {
    int titleLength = Encoding.Unicode.GetByteCount(title);
    byte[] result = new byte[4+4+titleLength+4];
    
    BinaryPrimitives.WriteInt32BigEndian(result, roomId);
    BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(4), titleLength);
    Encoding.Unicode.GetBytes(title).CopyTo(result, 4+4);
    BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(4+4+titleLength), curUserCount);
    
    return result;
  }
}