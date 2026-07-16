using System.Net.Sockets;
using System.Text;

namespace Client;

public class Sample2
{
  private string address = "127.0.0.1";
  private int port;
  private async Task ReceiveChat(TextReader reader, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      string? response = await reader.ReadLineAsync(token);
      if (response is null)
      {
        Console.WriteLine("Stream is disconnected by server. Shutting down...");
        return;
      }

      Console.WriteLine(response);
    }
  }

  private async Task SendChat(TextWriter writer, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      Console.Write("나: ");
      var message = await Console.In.ReadLineAsync(token);
      if (message.Equals("/end"))
      {
        Console.WriteLine("Shutting down...");
        return;
      }

      await writer.WriteLineAsync(message);
      // Thread.Sleep(500);
      // 비동기에서는 위에보다 아래를 쓰자
      await Task.Delay(100, token);
    }
  }
  public async Task StartClient()
  {
    Console.Write("Port number? (default: 50000): ");
    var tmp = Console.ReadLine();
    port = int.Parse(tmp.Equals("") ? "50000" : tmp);
    
    using var client = new TcpClient();
    
    // 연결 중
    Console.WriteLine($"Connecting to {address}:{port}...");
    await client.ConnectAsync(address, port);
    
    // 연결 완료 시
    Console.WriteLine($"Connected to {address}:{port}");

    await using NetworkStream stream = client.GetStream();
    
    using StreamReader reader = new(
      stream,
      Encoding.UTF8,
      detectEncodingFromByteOrderMarks: false,
      leaveOpen: true);
  
    await using StreamWriter writer = new(
      stream,
      new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
      leaveOpen: true)
    {
      AutoFlush = true
    };

    CancellationTokenSource cts = new();
    CancellationToken token = cts.Token;
    
    var receiver = ReceiveChat(reader, token);
    var sender = SendChat(writer, token);

    try
    {
      await Task.WhenAny(sender, receiver);
      cts.Cancel();
      await Task.WhenAll(sender, receiver);
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine("Connection closed. Shutdown...");
    }
  }
  public static async Task Main(string[] args)
  {
    Sample2 sample1 = new Sample2();
    try
    {
      await sample1.StartClient();
    }
    catch (SocketException ex)
    {
      Console.WriteLine(ex.Message);
    }
    Console.WriteLine("Done");
  }
}