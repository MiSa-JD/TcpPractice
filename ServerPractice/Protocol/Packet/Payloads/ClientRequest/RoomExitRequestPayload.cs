using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public class RoomExitRequestPayload: IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(
      PacketType.RoomExitRequest, []);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new RoomExitRequestPayload();
  }
}