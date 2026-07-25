using System.Buffers.Binary;
using System.Reflection;
using System.Text;

namespace Protocol.Packet.Payloads;

public record RoomOnPacket(
  int roomId,
  int titleLength,
  string title,
  int curUserCount,
  int maxUserCount)
{
  public RoomOnPacket(int roomId, string title, int curUserCount, int maxUserCount) :
    this(roomId, Encoding.Unicode.GetByteCount(title), title, curUserCount, maxUserCount) { }
  
  public byte[] Turn2Bytes()
  {
    byte[] result = new byte[4+4+titleLength+4+4];
    
    BinaryPrimitives.WriteInt32BigEndian(result, roomId);
    BinaryPrimitives.WriteInt32BigEndian(result, titleLength);
    Encoding.Unicode.GetBytes(title).CopyTo(result, 4+4);
    BinaryPrimitives.WriteInt32BigEndian(result, curUserCount);
    BinaryPrimitives.WriteInt32BigEndian(result, maxUserCount);
    
    return result;
  }
}