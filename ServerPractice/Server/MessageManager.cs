using Protocol.Connection;
using Protocol.Packet.Models;
using Protocol.Token;
using Server.Client;
using Server.Connection;

namespace Server;

public class MessageManager
{
  private MessageManager () { }
  
  public static MessageManager _instance { get; } = new();
  
  public async Task Broadcast(int? fromId,
    IEnumerable<ConnectionInfo> clients, string message)
  {
    
  }
  
  public async Task Unicast(ConnectionInfo src, ConnectionInfo dest, string message)
  {
    
  }
}