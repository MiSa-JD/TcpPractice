using System.Collections.Concurrent;
using System.Threading.Channels;
using Protocol.User;
using Server.Client;

namespace Server.Room;

public class RoomManager
{
  private RoomManager() { }
  public static RoomManager _instance { get; } = new();

  public ConcurrentDictionary<int, RoomInfo> rooms { get; } = new ConcurrentDictionary<int, RoomInfo>();
  public int AutoIncrease { get; } = 0;

  public bool CreateRoom(ClientInfo host, string title, int? maxUserCount)
  {
    return rooms.TryAdd(AutoIncrease,
      new RoomInfo(host, title, maxUserCount ?? 10));
  }
}