using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Server.Types;

namespace Server;

class ClientInfo: IAsyncDisposable
{
  public TextReader reader { get; }
  private TextWriter writer { get; }
  private Stream stream;
  private TcpClient client;
  public int id { get; }

  private Channel<string> channel = Channel.CreateBounded<string>(
    new BoundedChannelOptions(32)
  {
    SingleReader = true,
    SingleWriter = true,
    FullMode = BoundedChannelFullMode.DropOldest
  });
  private Task sendingTask;
  
  public static int AutoIncrease { get; private set; }
  
  public ClientInfo(TcpClient client, CancellationToken token)
  {
    this.client = client;
    stream = this.client.GetStream();
    reader = new StreamReader(
      stream,
      new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
      leaveOpen: true);
    writer = new StreamWriter(
      stream,
      new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
      leaveOpen: true)
    {
      AutoFlush = true
    };
    
    sendingTask = RunSender(token);
    id = ++AutoIncrease;
  }

  public async Task SendAsync(string message)
  {
      await channel.Writer.WriteAsync(message);
  }

  private async Task RunSender(CancellationToken token)
  {
    await foreach (var message in channel.Reader.ReadAllAsync(token))
    {
      await writer.WriteLineAsync(message);
    }
  }

  public async ValueTask DisposeAsync()
  {
    channel.Writer.Complete();
    try
    {
      await sendingTask;
    }
    catch (OperationCanceledException) { }
    catch (IOException) { }
    finally
    {
      writer.Dispose();
      reader.Dispose();
      stream.Dispose();
      client.Dispose();
    }
  }
}

public class Sample3
{
  private readonly int port = 50000;
  private ConcurrentDictionary<int, ClientInfo> clients = new();
  private CancellationTokenSource cts = new();

  private async Task<int> CommandManager(ClientInfo client, string message)
  {
    var messageSplit = message.Split(' ');
    try
    {
      switch (messageSplit[0])
      {
        case "/msg":
          int toId = int.Parse(messageSplit[1]);
          var realMessage = message.Replace($"{messageSplit[0]} {messageSplit[1]}", "");
          await SendMessage(client.id, toId, realMessage);
          return 0;
        case "/stop":
          await Broadcaster(client.id, $"Client {client.id} is stopped Server.");
          await cts.CancelAsync();
          return -1;
        default:
          await SendMessage(client.id, client.id, "Wrong Command!");
          return 1;
      }
    }
    catch (FormatException)
    {
      return 1;
    }
    catch (IndexOutOfRangeException)
    {
      await client.SendAsync("인자가 부족합니다!");
      return 1;
    }
    catch (OperationCanceledException) { }
    catch (Exception)
    {
      await client.SendAsync("잘못된 입력입니다!");
      return 1;
    }

    return 0;
  }
  
  private async Task RunReceiver(ClientInfo client, CancellationToken token)
  {
    try
    {
      while (!token.IsCancellationRequested)
      {
        string? message = await client.reader.ReadLineAsync(token);
        if (message is null)
        {
          string tmp = $"Client {client.id} is disconnected.";
          Console.WriteLine(tmp);
          await Broadcaster(null,"Server: "+tmp);
          break;
        }

        if (message.StartsWith('/'))
        {
          // 명령어 관리자 호출
          // -1: /stop
          // 1: 에러
          // 0: 내부에서 응답 처리
          // 나머지: 처리했으나 메세지를 정상적으로 보냄
          switch (await CommandManager(client, message))
          {
            case -1:
              return;
            case 0:
            case 1:
              continue;
          }
        }

        Console.WriteLine($"Client {client.id}: {message}");
        await Broadcaster(client.id, message);
      }
    }
    finally
    {
      if (clients.TryRemove(client.id, out var tmp))
        await tmp.DisposeAsync();
    }
  }

  private async Task Broadcaster(int? fromId, string message)
  {
    var tmp = clients.Values.ToArray();
    foreach (var client in tmp)
    {
      if (client.id == fromId)
        continue;
      if (fromId is null)
      {
        await client.SendAsync($"{message}");
        continue;
      }

      await client.SendAsync($"Client {fromId}: {message}");
      Console.WriteLine($"{fromId} -> {client.id}");
    }
  }

  private async Task SendMessage(int fromId, int toId, string message)
  {
    try
    {
      await clients[toId]
        .SendAsync($"*{fromId}: {message}*");
      Console.WriteLine($"*{fromId} -> {toId}*");
    }
    catch (KeyNotFoundException firstEx)
    {
      Console.WriteLine($"{firstEx}\n잘못된 id 호출!");
      try
      {
        await clients[fromId]
          .SendAsync("존재하지 않는 클라이언트입니다!");
      }
      catch (KeyNotFoundException secondEx)
      {
        Console.WriteLine($"{secondEx}\n잘못된 발송자입니다!");
      }
    }
  }
  
  private async Task ManageClients(TcpListener listener, CancellationToken token)
  {
    try
    {
      while (!token.IsCancellationRequested)
      {
        TcpClient client = await listener.AcceptTcpClientAsync(token);
        var tmp = new ClientInfo(client, token);
        if (!clients.TryAdd(ClientInfo.AutoIncrease, tmp))
        {
          _ = tmp.SendAsync("Something went wrong!\n");
          await tmp.DisposeAsync();
          continue;
        }

        _ = tmp.SendAsync($"Your Number is {ClientInfo.AutoIncrease}.\n");


        Console.WriteLine($"Client {ClientInfo.AutoIncrease} - {client.Client.RemoteEndPoint} connected");
        _ = HandleClient(tmp, token);
      }
    }
    catch (OperationCanceledException) { }
    finally
    {
      foreach (ClientInfo client in clients.Values)
      {
        clients.TryRemove(client.id, out ClientInfo? tmp);
        if (tmp is not null)
          await tmp.DisposeAsync();
      }
    }
  }
  
  private async Task StartListening()
  {
    TcpListener listener = new TcpListener(IPAddress.Loopback, port);
    listener.Start();
    Console.WriteLine("Listening on port " + port);
    
    CancellationToken token = cts.Token;

    // await Task.WhenAny(ManageClients(listener, token));
    // cts.Dispose();
    // await Task.WhenAll(ManageClients(listener, token));
    await ManageClients(listener, token);
  }
  public static async Task Main(string[] args)
  {
    Sample3 sample = new Sample3();
    await sample.StartListening();
    Console.WriteLine("Done");
  }
}