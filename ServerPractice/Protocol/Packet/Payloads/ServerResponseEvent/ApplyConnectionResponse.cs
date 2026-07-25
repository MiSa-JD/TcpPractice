using Protocol.Packet.Models;

namespace Protocol.Packet.Payloads.ServerResponseEvent;

// public record ApplyConnectionResponse(
//   Guid uuid,
//   int roomCount,
//   List<RoomOnPacket> rooms): IPayload
// {
//   public PacketInfo ToPacket()
//   {
//     var type = PacketType.ApplyConnectionResponse;
//     
//     foreach (var room in rooms)
//     {
//       
//     }
//   }
// }