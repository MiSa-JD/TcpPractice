using System.Net.Sockets;

internal class Program
{
  private const string address = "127.0.0.1";
  private const int port = 5000;

  private async Task StartClient()
  {
    TokenManager tokenM = TokenManager.GetManager();
    CancellationToken token = tokenM.GetToken();
    var connection = new TcpClient();
    
    // 연결 중
    Console.WriteLine("Waiting for connection...");
    await connection.ConnectAsync(address, port, token);
    
    // 연결 완료 시
    Console.WriteLine("Connected!");
    ClientInfo client = new ClientInfo(connection, token);
  }
  
  public static async Task Main(String[] args)
  {
    TcpPractice tcpPractice = new();
    await tcpPractice.StartClient();
    
  }
}