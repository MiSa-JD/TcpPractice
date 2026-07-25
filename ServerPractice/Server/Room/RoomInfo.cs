using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Runtime.InteropServices.ComTypes;
using Protocol.User;
using Server.Client;

namespace Server.Room;

public class RoomInfo
{
  private readonly UserInfo host;
  private readonly ConcurrentDictionary<int, ClientInfo> users = new();
  private int AutoIncrement = 0;

  public string title;
  public readonly int maxUserCount;

  public RoomInfo(ClientInfo host, string title, int maxUserCount)
  {
    this.host = host.user;
    this.title = title;
    this.maxUserCount = maxUserCount;
    AddUser(host);
  }
  
  public bool AddUser(ClientInfo client)
  {
    var result = maxUserCount > users.Count && users.TryAdd(AutoIncrement, client);
    ++AutoIncrement;
    return result;
  }

  public bool RemoveUser(int sessionId)
  {
    bool result = users.TryRemove(sessionId, out var client);
    if (client is null || !result)
      return result;
    client.tmpId = -1;
    return result;
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
  public int GetMaxUserCount()
  {
    return maxUserCount;
  }
}