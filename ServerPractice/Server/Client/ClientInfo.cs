using Protocol.Connection;
using Protocol.Packet.Models;
using Protocol.User;
using Server.Room;

namespace Server.Client;

public class ClientInfo: IAsyncDisposable
{
  public ClientInfo(ConnectionInfo connection, UserInfo user)
  {
    this.connection = connection;
    this.user = user;
    MoveRoom(RoomManager._instance.lobby);
  }
  public ConnectionInfo connection { get; }
  public UserInfo user { get; }

  public RoomInfo currentRoom { get; private set; }
    = RoomManager._instance.lobby;
  public int roomUserId = 0;
  public Task SendAsync(PacketInfo packet) => connection.SendAsync(packet);
  
  public async Task MoveRoom(RoomInfo room)
  {
    await room.AddUser(this);
    currentRoom = room;
  }
  public async ValueTask DisposeAsync()
  {
    Console.WriteLine("Disconnected with " + connection.GetAddress());
    await currentRoom.RemoveUser(this);
    await connection.DisposeAsync();
  }
}