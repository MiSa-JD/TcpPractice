using System.Net.Sockets;
using System.Text;

namespace Client;

class ClientInfo: IAsyncDisposable
{
  public TcpClient client { get; }
  private Stream stream;
  public TextReader reader { get; }
  public TextWriter writer { get; }

  public ClientInfo(TcpClient client)
  {
    this.client = client;
    stream = this.client.GetStream();
    
    reader = new StreamReader(
      stream,
      Encoding.UTF8,
      detectEncodingFromByteOrderMarks: false,
      leaveOpen: true);
    
    writer = new StreamWriter(
      stream,
      new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
      leaveOpen: true)
    {
      AutoFlush = true
    };
  }

  public async ValueTask DisposeAsync()
  {
    writer.Dispose();
    reader.Dispose();
    stream.Dispose();
    client.Dispose();
  }
}

public class Sample3
{
  private string address = "127.0.0.1";
  private int port;
  
  // 메세지 받는 Task
  private async Task ReceiveMessage(TextReader reader, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      string? response = await reader.ReadLineAsync(token);
      // response is null: 연결 종료
      if (response is null)
      {
        Console.WriteLine("Stream is disconnected by server. Shutting down...");
        return;
      }

      Console.WriteLine(response);
    }
  }
  // 보낼 메세지 받는 Task
  private async Task SendMessage(ClientInfo client, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      Console.Write("나: ");
      var message = await Console.In.ReadLineAsync(token);
      if (message is null)
      {
        Console.WriteLine("Wrong Input!");
        continue;
      }
      
      if (message.Equals(""))
      {
        Console.WriteLine("Message is empty.");
        continue;
      }
      
      if (message.Equals("/end") || token.IsCancellationRequested)
      {
        Console.WriteLine("Exiting...");
        return;
      }
      
      await client.writer.WriteLineAsync(message);
      await Task.Delay(100, token);
    }
  }
  
  private async Task StartClient()
  {
    // 포트 선택
    while (true)
    {
      Console.Write("Port number? (1~65535, default: 50000): ");
      try
      {
        var tmp = Console.ReadLine();
        if (tmp is null)
        {
          Console.WriteLine("Wrong input!");
          continue;
        }
        port = int.Parse(tmp.Equals("") ? "50000" : tmp);
        if (port < 1 || port > 65535)
        {
          Console.WriteLine("Port number is invalid.");
          continue;
        }
        break;
      }
      catch (FormatException)
      {
        Console.WriteLine("Port number is invalid.");
      }
    }
    
    // 클라이언트 생성
    CancellationTokenSource cts = new();
    CancellationToken token = cts.Token;
    using var connection = new TcpClient();
    
    // 연결 중
    Console.WriteLine($"Connecting to {address}:{port}...");
    await connection.ConnectAsync(address, port, token);
    
    // 연결 완료 시
    Console.WriteLine($"Connected to {address}:{port}");
    ClientInfo client = new ClientInfo(connection);
    
    var receiver = ReceiveMessage(client.reader, token);
    var sender = SendMessage(client, token);

    try
    {
      await Task.WhenAny(sender, receiver);
      await cts.CancelAsync();
      await Task.WhenAll(sender, receiver);
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine("Connection closed. Shutting down...");
    }
    finally
    {
      await client.DisposeAsync();
    }
  }
  public static async Task Main(string[] args)
  {
    Sample3 sample3 = new();
    try
    {
      await sample3.StartClient();
    }
    catch (SocketException)
    {
      Console.WriteLine("Server is closed. Shutting down...");
    }
    Console.WriteLine("Done");
  }
}