using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerRequest;

public class UsernameRequest: IPayload
{
  public PacketInfo ToPacket()
  {
    return PacketCodec
      .Data2Packet(PacketType.UsernameRequest,[]);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new UsernameRequest();
  }
}