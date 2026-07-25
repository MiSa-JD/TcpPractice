using Protocol.Connection;
using Server.Client;
using Server.Connection;

namespace Server;

public class MessageManager
{
  private MessageManager () { }
  
  private static MessageManager? _manager;
  
  public static MessageManager GetManager() { return _manager ??= new MessageManager(); }
  public async Task Broadcast(int? fromId,
    IEnumerable<ConnectionInfo> clients, string message)
  {
    
  }
  
  public async Task Unicast(ConnectionInfo src, ConnectionInfo dest, string message)
  {
    
  }
}