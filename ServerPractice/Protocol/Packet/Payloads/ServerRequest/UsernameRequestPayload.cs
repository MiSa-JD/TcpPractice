using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerRequest;

public class UsernameRequestPayload: IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(
      PacketType.UsernameRequest);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new UsernameRequestPayload();
  }
}