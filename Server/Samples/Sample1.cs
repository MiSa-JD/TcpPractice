using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;


public class Sample1
{
  const int port = 50000;
  
  public async Task StartListening()
  {
    TcpListener listener = new TcpListener(IPAddress.Loopback, port);
    listener.Start();
    Console.WriteLine("Listening on port " + port);
    
    int tryCount = 0;
    while (tryCount++ < 100)
    {
      using TcpClient client = await listener.AcceptTcpClientAsync();
     
      Console.WriteLine("Connected");
      
      // Stream
      await using var stream = client.GetStream();
      
      // Reader, Writer
      using StreamReader reader = new (
        stream,
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        leaveOpen: true);

      await using StreamWriter writer = new(
        stream,
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        leaveOpen: true)
      {
        AutoFlush = true
      };
      
      // 5번만 돌리기
      int msgCount = 0;
      while (true)
      {
        string? message = await reader.ReadLineAsync();
        if (message is null)
        {
          Console.WriteLine("Client disconnected");
          break;
        }

        Console.WriteLine($"클라이언트: {message}");

        string response = $"서버: {message}";
        ++msgCount;

        // 주고 받은 횟수가 5번을 넘기면 끊기
        if (msgCount > 5)
        {
          string tmm = response+"\nToo many messages. Disconnecting...\n"+tryCount;
          Console.WriteLine(tmm);
          await writer.WriteLineAsync(tmm);
          client.Close();
          break;
        }
        await Task.Delay(500);
        Console.WriteLine(response);
        await writer.WriteLineAsync(response);

        Console.WriteLine("Complete sending response");
      }
      
    }
    listener.Dispose();
  }
  
  public static async Task Main(string[] args)
  {
    Sample1 sample = new Sample1();
    await sample.StartListening();
    Console.WriteLine("Done");
  }
}