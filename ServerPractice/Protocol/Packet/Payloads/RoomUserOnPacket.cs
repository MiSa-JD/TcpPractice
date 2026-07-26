using System.Buffers.Binary;
using System.Text;

namespace Protocol.Packet.Payloads;

public record RoomUserOnPacket(int roomUserId, string username): ISubclassesOfPacket
{
  public byte[] Turn2Bytes()
  {
    int nameLength = Encoding.UTF8.GetByteCount(username);
    byte[] result = new byte[4+4+nameLength];
    
    BinaryPrimitives.WriteInt32BigEndian(result, roomUserId);
    BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(4), nameLength);
    Encoding.UTF8.GetBytes(username).CopyTo(result, 4+4);
    
    return result;
  }
}