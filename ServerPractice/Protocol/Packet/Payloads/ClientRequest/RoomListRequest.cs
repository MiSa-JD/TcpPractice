using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public class RoomListRequest: IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(
      PacketType.RoomListRequest,
      []);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new RoomListRequest();
  }
}