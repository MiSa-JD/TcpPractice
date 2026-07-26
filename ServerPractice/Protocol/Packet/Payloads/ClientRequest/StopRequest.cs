using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public class StopRequest: IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(PacketType.StopRequest, []);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new StopRequest();
  }
}