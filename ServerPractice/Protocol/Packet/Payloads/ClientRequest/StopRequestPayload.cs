using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public class StopRequestPayload: IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(PacketType.StopRequest, []);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new StopRequestPayload();
  }
}