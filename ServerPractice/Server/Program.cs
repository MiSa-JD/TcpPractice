using System.Net;
using System.Net.Sockets;
using Protocol.SystemMessage;
using Server;
using Protocol.Packet;

internal class Program
{
  private const int port = 5000;
  private async Task StartListening()
  {
    TcpListener listener = new(IPAddress.Loopback, port);
    listener.Start();
    Console.WriteLine("Listening on " + IPAddress.Loopback + ":" + port);

    await ManageClients(listener);
  }
  
  private async Task ManageClients(TcpListener listener)
  {
    TokenManager tokenM = TokenManager.GetManager();
    ClientManager clientM = ClientManager.GetManager();
    
    try
    {
      CancellationToken token = tokenM.GetCt();
      while (!token.IsCancellationRequested)
      {
        var connection = await listener.AcceptTcpClientAsync(token);
        
        // 최대 연결 풀 초과 시
        if (ClientManager.maxClient <= clientM.GetCount())
        {
          var tmp = PacketEncoder.EncodeSystemMessage(
                (char)SystemMessageType.Error,
                "The Connection Pool was Full");
          await connection.GetStream()
            .WriteAsync(
              PacketCodec.GetManager().Packet2Bytes(tmp), 
              token);
          connection.Dispose();
          continue;
        }
        
        // 연결 성공
        var client = new ClientInfo(connection, token);
        if (!clientM.TryAdd(client))
        {
          await client.SendAsync(
            PacketEncoder.EncodeSystemMessage((char)SystemMessageType.Error, "Something went wrong!")
          );
          await client.DisposeAsync();
          continue;
        }
        
        await client.SendAsync(
          PacketEncoder.EncodeSystemMessage((char)SystemMessageType.Info, "Connection Complete.")
        );
        Console.WriteLine($"Client {connection.Client.RemoteEndPoint} connected");
        
      }
    }
    catch (OperationCanceledException) { }
    finally
    {
      listener.Stop();
      await tokenM.stop();
      await clientM.ClearClients();
    }
  }
  
  public static async Task Main(String[] args)
  {
    Program server = new();
    await server.StartListening();
    Console.WriteLine("Done");
  }
}