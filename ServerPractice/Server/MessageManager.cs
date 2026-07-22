namespace Server;

public class MessageManager
{
  private MessageManager () { }
  
  private static MessageManager? _manager;
  
  public static MessageManager GetManager() { return _manager ??= new MessageManager(); }
  public async Task Broadcast(int? fromId,
    IEnumerable<ClientInfo> clients, string message)
  {
    
  }
  
  public async Task Unicast(ClientInfo src, ClientInfo dest, string message)
  {
    
  }
}