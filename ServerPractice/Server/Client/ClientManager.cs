using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Protocol.Connection;
using Protocol.Packet.Models;
using Protocol.Packet.Payloads.ServerResponseEvent;
using Protocol.Packet.Payloads.ServerRequest;
using Protocol.SystemMessage;
using Protocol.Token;
using Protocol.User;
using Server.Room;
using Server.User;

namespace Server.Client;

public class ClientManager: IAsyncDisposable
{
  private ClientManager() { runTask = Run(); }
  public static ClientManager _instance { get; } = new();

  private ConcurrentDictionary<Guid, ClientInfo> clients { get; } = [];
  private ConcurrentDictionary<int, ConnectionInfo> connections { get; } = [];
  private static int AutoIncrement;
  private const int maxConnection = 5;
  private readonly Channel<Task> tasks = Channel.CreateUnbounded<Task>();
  private ValueTask enqueue(Task task, CancellationToken token) => tasks.Writer.WriteAsync(task, token);

  private readonly Task runTask;

  private async Task Run()
  {
    await foreach (var task in tasks.Reader.ReadAllAsync(TokenManager._instance.token))
    {
      try
      {
        await task;
      }
      catch (OperationCanceledException) { }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }
  }
  
  public async Task ManageClients(TcpListener listener)
  {
    CancellationToken token = TokenManager._instance.token;
    
    var tcpClient = await listener.AcceptTcpClientAsync(token);

    // 연결 성공
    var connection = new ConnectionInfo(tcpClient, token);
    connection.connectionId = Interlocked.Increment(ref AutoIncrement);
    
    // 최대 연결 풀 초과 시
    if (!TryAddConnection(connection))
    {
      var tmp = new SystemMessagePayload(
        SystemMessageType.Error,
        "Too Much Connections!");
      
      await connection.SendAsync(tmp.ToPacket());
      await connection.DisposeAsync();
      return;
    }

    var msg = new SystemMessagePayload(SystemMessageType.Info, "Connected.");
    await connection.SendAsync(msg.ToPacket());
    
    Console.WriteLine($"Client {tcpClient.Client.RemoteEndPoint} connected");
    await enqueue(AskUsername(connection), TokenManager._instance.token);
  }

  private async Task AskUsername(ConnectionInfo connection)
  {
    Console.WriteLine("Asking Username...");
    await connection.SendAsync(new UsernameRequestPayload().ToPacket());
    
    string name;
    
    while (true)
    {
      var packet = await connection.ReceiveAsync(TokenManager._instance.token);
      // 연결 끊김
      if (packet is null)
      {
        Console.WriteLine("Connection closed by client " + connection.GetAddress());
        return;
      }

      // 유저 이름을 못 받음
      if (packet.type != PacketType.UsernameResponse)
      {
        await connection.SendAsync(new SystemMessagePayload(
          SystemMessageType.Error,
          "Wrong Response!").ToPacket());
        await connection.DisposeAsync();
        throw new Exception("Invalid Packet!");
      }

      name = Encoding.UTF8.GetString(packet.payload);
      // TODO: 해당 이름 접속 여부 체크
      if (false)
      {
        var tmp = new SystemMessagePayload(
          SystemMessageType.Warn,
          "해당 유저는 이미 접속하고 있습니다!");
        await connection.SendAsync(tmp.ToPacket());
        continue;
      }

      break;
    }

    Guid uuid = UserManager._instance.SearchOrCreate(name);
    var user = new UserInfo(uuid, name);
    var client = new ClientInfo(connection, user);
    clients.TryAdd(uuid, client);
    
    Console.WriteLine("Username: " + name + " / UUID: " + uuid);
    
    await client.SendAsync(
      new ApplyConnectionResponsePayload(
          uuid,
          RoomManager._instance.GetRoomsForPacket())
        .ToPacket());
    Console.WriteLine("Send uuid and room list to " + client.user.uuid);
    await enqueue(RoomManager._instance.ProcessPackets(client), TokenManager._instance.token);
  }

  public async Task RemoveClient(Guid uuid)
  {
    clients.TryRemove(uuid, out var target);
    if (target == null)
      return;
    connections.TryRemove(target.connection.connectionId, out _);
    
    await target.DisposeAsync();
  }

  private bool TryAddConnection(ConnectionInfo connection)
  {
    return connections.Count < maxConnection 
           && connections.TryAdd(connection.connectionId, connection);
  }

  public async ValueTask DisposeAsync()
  {
    runTask.Dispose();
    foreach (var client in clients.Values)
    {
      await client.DisposeAsync();
    }

    clients.Clear();
  }
}