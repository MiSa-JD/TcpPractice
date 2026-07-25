using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads;

public interface IPayload
{
  public PacketInfo ToPacket();
  public static abstract void ReadBytes(byte[] bytes, out IPayload payload);
}