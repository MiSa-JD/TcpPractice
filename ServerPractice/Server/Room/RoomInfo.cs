using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Runtime.InteropServices.ComTypes;
using Protocol.Packet.Models;
using Protocol.Packet.Payloads;
using Protocol.Packet.Payloads.ServerResponseEvent;
using Protocol.User;
using Server.Client;

namespace Server.Room;

public class RoomInfo
{
  private ClientInfo? host;
  private readonly ConcurrentDictionary<int, ClientInfo> users = new();
  private int AutoIncrement = 0;
  public string title { get; }

  public RoomInfo(ClientInfo client, string title)
  {
    host = client;
    this.title = title;
    AddUser(client);
  }

  public RoomInfo(string title)
  {
    host = null;
    this.title = title;
  }
  
  public bool AddUser(ClientInfo client)
  {
    var result = users.TryAdd(AutoIncrement, client);
    client.roomUserId = AutoIncrement;
    ++AutoIncrement;
    return result;
  }

  public async Task RemoveUser(ClientInfo client)
  {
    users.TryRemove(client.roomUserId, out _);
    string msg = client.user.username + " left from " + title;
    Console.WriteLine(msg);
    await BroadcastInRoom(client,
      new BroadcastEventPayload(client.user.username, msg)
        .ToPacket());
  }

  public async Task UserLeft(ClientInfo client)
  {
    await RemoveUser(client);
    client.MoveRoom(RoomManager._instance.lobby);
    if (host != null && client.Equals(host))
      foreach (var user in users)
        await UserLeft(client);
  }

  public Dictionary<int, UserInfo> GetUserList()
  {
    Dictionary<int, UserInfo> tmp = [];
    
    foreach (var user in users)
      tmp.TryAdd(user.Key, user.Value.user);
    
    return tmp;
  }

  public int GetCount()
  {
    return users.Count;
  }

  public async Task BroadcastInRoom(ClientInfo client, PacketInfo packet)
  {
    foreach (var user in users.Values)
    {
      if (client.Equals(user)) continue;
      await user.SendAsync(packet);
    }
  }
}