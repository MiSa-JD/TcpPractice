using System.Runtime.CompilerServices;
using Protocol.Connection;
using Protocol.User;
using Server.Connection;

namespace Server.Client;

public class ClientInfo(ConnectionInfo connection, UserInfo user): IAsyncDisposable
{
  public ConnectionInfo connection { get; } = connection;
  public UserInfo user { get; } = user;
  public int roomid = -1;
  public int tmpId = -1;
  public async ValueTask DisposeAsync()
  {
    Console.WriteLine("Disconnected with " + connection.GetAddress());
    await connection.DisposeAsync();
  }
}