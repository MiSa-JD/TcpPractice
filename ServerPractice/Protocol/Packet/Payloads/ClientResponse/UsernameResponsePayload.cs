using System.Text;
using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ClientResponse;

public record UsernameResponsePayload(string username): IPayload
{
  public PacketInfo ToPacket()
  {
    return new PacketInfo(
      PacketType.UsernameResponse,
      Encoding.UTF8.GetBytes(username));
  }

  public static void ReadBytes(byte[] bytes, out IPayload payload)
  {
    string name = Encoding.UTF8.GetString(bytes);
    payload = new UsernameResponsePayload(name);
  }
}