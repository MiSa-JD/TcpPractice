using System.Buffers.Binary;
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
using Server.Connection;
using Server.User;

namespace Server.Client;

public class ClientManager: IAsyncDisposable
{
  private ClientManager() { runTask = Run(); }
  public static ClientManager _instance { get; } = new();

  public ConcurrentDictionary<Guid, ClientInfo> clients { get; } = [];
  private readonly Channel<Task> tasks = Channel.CreateUnbounded<Task>();
  private ValueTask enqueue(Task task, CancellationToken token) => tasks.Writer.WriteAsync(task, token);

  private readonly Task runTask;

  private async Task Run()
  {
    await foreach (var task in tasks.Reader.ReadAllAsync(TokenManager._instance.token))
    {
      await task;
    }
  }
  
  public async Task ManageClients(TcpListener listener)
  {
    CancellationToken token = TokenManager._instance.token;
    ConnectionManager connectionM = ConnectionManager._instance;
    
    var tcpClient = await listener.AcceptTcpClientAsync(token);

    // 연결 성공
    var client = new ConnectionInfo(tcpClient, token);
    
    // 최대 연결 풀 초과 시
    if (!connectionM.TryAdd(client))
    {
      var tmp = new SystemMessagePayload(
        SystemMessageType.Error,
        "Something went wrong!");
      
      await client.SendAsync(tmp.ToPacket());
      await client.DisposeAsync();
      return;
    }

    var msg = new SystemMessagePayload(SystemMessageType.Info, "Connected.");
    await client.SendAsync(msg.ToPacket());
    
    Console.WriteLine($"Client {tcpClient.Client.RemoteEndPoint} connected");
    await enqueue(AskUsername(client), TokenManager._instance.token);
  }

  private async Task AskUsername(ConnectionInfo connection)
  {
    Console.WriteLine("Asking Username...");
    await connection.SendAsync(new UsernameRequest().ToPacket());

    while (true)
    {
      byte[] buffer = new byte[4];
      await connection.stream.ReadExactlyAsync(buffer, TokenManager._instance.token);
      int length = BinaryPrimitives.ReadInt32BigEndian(buffer);
      
      buffer = new byte[2];
      await connection.stream.ReadExactlyAsync(buffer, TokenManager._instance.token);
      PacketType type = (PacketType)BinaryPrimitives.ReadInt16BigEndian(buffer);
      
      if (type != PacketType.UsernameResponse)
      {
        var tmp = new SystemMessagePayload(
          SystemMessageType.Error, 
          "Wrong Response Type!");
        await connection.SendAsync(tmp.ToPacket());
        await connection.DisposeAsync();
        return;
      }
      
      buffer = new byte[length];
      await connection.stream.ReadExactlyAsync(buffer, TokenManager._instance.token);
      string name = Encoding.UTF8.GetString(buffer);
      
      // TODO: 해당 이름 접속 여부 체크
      if (false)
      {
        var tmp = new SystemMessagePayload(
          SystemMessageType.Warn,
          "해당 유저는 이미 접속하고 있습니다!");
        await connection.SendAsync(tmp.ToPacket());
        continue;
      }

      Guid uuid = UserManager._instance.SearchOrCreate(name);
      var user = new UserInfo(uuid, name);
      var client = new ClientInfo(connection, user);
      clients.TryAdd(uuid, client);
      
      Console.WriteLine("Username: " + name + " / UUID: " + uuid);
      
      // 이것이 당신의 uuid 입니다
      // + 이것이 우리의 방 목록입니다
      await client.connection.SendAsync(
        new SystemMessagePayload(
          SystemMessageType.Info,
          "This is your UUID")
          .ToPacket());
      await client.connection.SendAsync(
        new SystemMessagePayload(
            SystemMessageType.Info,
            "This is Room List")
          .ToPacket());
    }
  }

  public async ValueTask DisposeAsync()
  {
    runTask.Dispose();
    foreach (var client in clients.Values)
      await client.DisposeAsync();
    clients.Clear();
  }
}