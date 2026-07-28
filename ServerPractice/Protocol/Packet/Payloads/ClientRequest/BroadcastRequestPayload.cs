using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientRequest;

public record BroadcastRequestPayload(string message) : IPayload
{
  public PacketInfo ToPacket()
  {
    byte[] payload = Encoding.Unicode.GetBytes(message);
    return new PacketInfo(
      PacketType.BroadcastRequest,
      payload);
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    string msg = Encoding.Unicode.GetString(bytes);
    payload = new BroadcastRequestPayload(msg);
  }
}