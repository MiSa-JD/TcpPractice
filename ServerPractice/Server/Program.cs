using System.Net;
using System.Net.Sockets;
using Protocol.Token;
using Server.Client;

namespace Server;

internal class Program
{
  private const int port = 50000;
  private async Task StartListening()
  {
    TcpListener listener = new(IPAddress.Loopback, port);
    listener.Start();
    Console.WriteLine("listening on " + IPAddress.Loopback + ":" + port);
    
    try
    {
      while (!TokenManager._instance.token.IsCancellationRequested)
      {
        await ClientManager._instance.ManageClients(listener);
      }
    }
    catch (OperationCanceledException) { }
    finally
    {
      listener.Stop();
    }
  }
  
  public static async Task Main(String[] args)
  {
    Program server = new();
    await server.StartListening();
    Console.WriteLine("Done");
  }
}