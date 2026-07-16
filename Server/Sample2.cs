using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

class ClientInfo
{
  public TextReader reader { get; }
  public TextWriter writer { get; }
  public static int AutoIncrease { get; private set; } = 0;
  public int id { get; }
  public ClientInfo(TcpClient client)
  {
    var stream = client.GetStream();
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

    
    id = ++AutoIncrease;
  }
}

public class Sample2
{
  private int port = 50000;
  private Dictionary<int, ClientInfo> clients;

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
          Broadcaster(null,"Server: "+tmp);
          break;
        }

        var messageSplit = message.Split(' ');
        if (messageSplit[0].Equals("/msg"))
        {
          try
          {
            int toId = int.Parse(messageSplit[1]);
            var realMessage = message.Replace($"{messageSplit[0]} {messageSplit[1]}", "");
            SendMessage(client.id, toId, realMessage);
          }
          catch (FormatException)
          {
            Console.WriteLine("");
          }
        }
        else
        {
          Console.WriteLine($"Client {client.id}: {message}");
          Broadcaster(client.id, message);
        }
      }
    }
    finally
    {
      clients.Remove(client.id);
      client.writer.Dispose();
      client.reader.Dispose();
    }
  }

  private async Task Broadcaster(int? fromId, string message)
  {
    var tasks = new List<Task>();
    foreach (var client in clients.Values)
    {
      if (client.id == fromId)
        continue;
      tasks.Add(Task.Run(async () =>
      {
        if (fromId is not null)
        {
          await client.writer.WriteLineAsync($"Client {fromId}: {message}");
          Console.WriteLine($"{fromId} -> {client.id}");
        }
        else 
          await client.writer.WriteLineAsync($"{message}");
      }));
    }
    
    await Task.WhenAll(tasks);
  }

  private async Task SendMessage(int fromId, int toId, string message)
  {
    try
    {
      await clients[toId]
        .writer.WriteLineAsync($"*{fromId}: {message}*");
      Console.WriteLine($"*{fromId} -> {toId}*");
    }
    catch (KeyNotFoundException firstEx)
    {
      Console.WriteLine($"{firstEx}\n잘못된 id 호출!");
      try
      {
        await clients[fromId]
          .writer.WriteLineAsync("존재하지 않는 클라이언트입니다!");
      }
      catch (KeyNotFoundException secondEx)
      {
        Console.WriteLine($"{secondEx}\n잘못된 발송자입니다!");
      }
    }
  }
  
  private async Task ManageClients(TcpListener listener, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      TcpClient client = await listener.AcceptTcpClientAsync(token);
      var tmp = new ClientInfo(client);
      clients.Add(ClientInfo.AutoIncrease, tmp);
      
      Console.WriteLine($"Client {ClientInfo.AutoIncrease} - {client.Client.RemoteEndPoint} connected");
      RunReceiver(tmp, token);
    }
  }
  
  public async Task StartListening()
  {
    TcpListener listener = new TcpListener(IPAddress.Loopback, port);
    listener.Start();
    Console.WriteLine("Listening on port " + port);

    clients = new Dictionary<int, ClientInfo>();
    
    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken token = cts.Token;

    await Task.WhenAny(ManageClients(listener, token));
    cts.Dispose();
    await Task.WhenAll(ManageClients(listener, token));
  }
  public static async Task Main(string[] args)
  {
    Sample2 sample = new Sample2();
    await sample.StartListening();
    Console.WriteLine("Done");
  }
}