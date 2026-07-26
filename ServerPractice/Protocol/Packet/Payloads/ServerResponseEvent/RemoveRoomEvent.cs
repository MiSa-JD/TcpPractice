using System.Buffers.Binary;
using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record RemoveRoomEvent(int roomNum, string title): IPayload
{
  public PacketInfo ToPacket()
  {
    int titleLength = Encoding.Unicode.GetByteCount(title);
    byte[] buffer = new byte[4+titleLength+4];
    
    BinaryPrimitives.WriteInt32BigEndian(buffer, roomNum);
    BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(4), titleLength);
    Encoding.Unicode.GetBytes(title).CopyTo(buffer.AsSpan(4+4));

    return new PacketInfo(
      PacketType.RemoveRoomEvent,
      buffer);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    int roomNum = BinaryPrimitives.ReadInt32BigEndian(bytes);
    int titleLength = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(4));
    string title = Encoding.Unicode.GetString(bytes.AsSpan(4+4, titleLength));
    
    payload = new RemoveRoomEvent(roomNum, title);
  }
}