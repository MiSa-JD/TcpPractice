using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public class DisconnectingRequest: IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(
      PacketType.DisconnectingRequest,
      []);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new DisconnectingRequest();
  }
}