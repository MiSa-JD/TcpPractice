using System.Buffers.Binary;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public record RoomEntranceRequestPayload(int roomid): IPayload
{
  public PacketInfo ToPacket()
  {
    byte[] payload = new byte[4];
    BinaryPrimitives.WriteInt32BigEndian(payload, roomid);
    return new PacketInfo(
      PacketType.RoomEntranceRequest,
      payload);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new RoomEntranceRequestPayload(
      BinaryPrimitives.ReadInt32BigEndian(bytes));
  }
}