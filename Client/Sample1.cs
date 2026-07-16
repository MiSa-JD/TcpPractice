using System.Net.Sockets;
using System.Text;

namespace Client;

public class Sample1
{
  const int port = 50000;
  const string address = "127.0.0.1";
  
  public async Task StartClient()
  {
    using TcpClient client = new TcpClient();
    
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

    string message = "";
    while (!message.Equals("/end"))
    {
      Console.Write("나: ");
      message = Console.ReadLine() ?? "message";
      await writer.WriteLineAsync(message);
      
      string? response = await reader.ReadLineAsync();
      if (response is null)
      {
        break;
      }
      Console.WriteLine(response);
    }
  }
  public static async Task Main(string[] args)
  {
    Sample1 sample1 = new Sample1();
    await sample1.StartClient();
    Console.WriteLine("Done");
  }
}