using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

public record DenyEntranceResponse() : IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(PacketType.DenyEntranceResponse, []);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    payload = new DenyEntranceResponse();
  }
}