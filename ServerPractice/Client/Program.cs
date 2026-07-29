using System.Net.Sockets;
using Client.Behavior;
using Protocol.Connection;
using Protocol.Token;

namespace Client;

internal class Program
{
  private const string address = "127.0.0.1";
  private const int port = 50000;

  private async Task StartClient()
  {
    CancellationToken token = TokenManager._instance.token;
    var tcpClient = new TcpClient();
    
    // 연결 중
    Console.WriteLine("Waiting for connection...");
    await tcpClient.ConnectAsync(address, port, token);
    
    // 연결 완료 시
    Console.WriteLine("Connected!");
    
    ConnectionInfo connection = new(tcpClient, token);

    var receiver = BehaviorManager._instance.ReceivePackets(connection);
    var sender = BehaviorManager._instance.GetUserInput(connection);
    try
    {
      await Task.WhenAny(sender, receiver);
      await TokenManager._instance.DisposeAsync();
    }
    catch (OperationCanceledException) { }
  }
  
  public static async Task Main(String[] args)
  {
    Program program = new();
    await program.StartClient();
    
  }
}