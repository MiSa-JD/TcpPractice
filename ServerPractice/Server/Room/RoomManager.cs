using System.Collections.Concurrent;
using Protocol.Packet.Models;
using Protocol.Packet.Payloads;
using Protocol.Packet.Payloads.ClientRequest;
using Protocol.Packet.Payloads.ServerResponseEvent;
using Protocol.Token;
using Server.Client;

namespace Server.Room;

public class RoomManager
{
  private RoomManager() { rooms.TryAdd(-1, lobby); }
  public static RoomManager _instance { get; } = new();

  public RoomInfo lobby { get; } = new("Lobby");
  private ConcurrentDictionary<int, RoomInfo> rooms { get; } = new();
  public int AutoIncrease { get; private set; } = 0;

  public List<RoomOnPacket> GetRoomsForPacket()
  {
    return rooms.Select(room 
      => new RoomOnPacket(room.Key, room.Value.title, room.Value.GetCount()))
      .ToList();
  }
  public int GetRoomCount() { return rooms.Count; }
  
  public async Task ProcessPackets(ClientInfo client)
  {
    while (!TokenManager._instance.token.IsCancellationRequested)
    {
      var receivedPacket = await client.connection.ReceiveAsync(TokenManager._instance.token);
      if (receivedPacket is null)
      {
        await ClientManager._instance.RemoveClient(client.user.uuid);
        return;
      }

      Console.WriteLine($"{client.user.username}: {receivedPacket.type}");
      switch (receivedPacket.type)
      {
        case PacketType.BroadcastRequest:
          await SendBroadcast(client, receivedPacket);
          break;
        case PacketType.UnicastRequest:
          await SendUnicast(client, receivedPacket);
          break;
        case PacketType.RoomEntranceRequest:
          break;
      }
    }
  }

  private async Task SendBroadcast(ClientInfo client, PacketInfo receivedPacket)
  {
    var sendingPacket = new BroadcastEventPayload(
      client.roomUserId,
      receivedPacket.payload);
    await client.currentRoom.BroadcastInRoom(client, sendingPacket.ToPacket());
  }

  private async Task SendUnicast(ClientInfo client, PacketInfo receivedPacket)
  {
    UnicastRequestPayload.ReadBytes(receivedPacket.payload, out var _payload);
    var payload = (UnicastRequestPayload)_payload;
    // TODO: Broadcast, Unicast 이름 기반 -> roomUserId 기반으로 구조 바꿀 때 당장 바꿔야 함 
    var sendingPacket = new UnicastEventPayload(
      client.roomUserId,
      payload.message).ToPacket();
    await client.currentRoom.UnicastInRoom(
      client.currentRoom.GetUserList()[payload.receiverId].username, sendingPacket);
  }
}