using System.Collections.Concurrent;
using Protocol.Packet.Payloads;

namespace Client.User;

public class UserMapper
{
  private UserMapper() { }
  public static UserMapper _instance { get; } = new();
  private ConcurrentDictionary<int, string> users = new();

  public Task AddUser(int roomUserId, string username)
  {
    Console.WriteLine(roomUserId + ": " + username + " has joined");
    users.TryAdd(roomUserId, username);
    return Task.CompletedTask;
  }

  public Task RemoveUser(int roomUserId)
  {
    Console.WriteLine(users[roomUserId] + " has left");
    users.TryRemove(roomUserId, out _);
    return Task.CompletedTask;
  }

  public int GetUserByName(string username)
  {
    foreach (var user in users)
      if (user.Value.Equals(username))
        return user.Key;
    throw new KeyNotFoundException();
  }

  public string GetUserByRoomUserId(int roomUserId)
  {
    return users[roomUserId];
  }

  public void PrintUserList()
  {
    Console.WriteLine("현재 인원 수: " + users.Count);
    foreach (var user in users)
      Console.WriteLine(" "+user.Key+": "+user.Value);
  }

  public bool IsRoomUser(string username)
  {
    try
    {
      GetUserByName(username);
      return true;
    }
    catch (KeyNotFoundException)
    {
      Console.WriteLine("There is no room user: " + username);
      return false;
    }
  }

  public Task InitializeUser(List<RoomUserOnPacket> userList)
  {
    users = new ConcurrentDictionary<int, string>();
    Console.WriteLine("사람 수: " + userList.Count);
    foreach (var user in userList)
      AddUser(user.roomUserId, user.username);
    return Task.CompletedTask;
  }
  
}