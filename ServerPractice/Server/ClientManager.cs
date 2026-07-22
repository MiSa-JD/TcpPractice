using System.Collections.Concurrent;

namespace Server;

public class ClientManager
{
  private ClientManager () { }
  
  public const int maxClient = 5;
  private static ClientManager? _manager;
  private static readonly ConcurrentBag<ClientInfo> clients = [];

  public static ClientManager GetManager() { return _manager ??= new ClientManager(); }
  
  public int GetCount ()
    => clients.Count;
  
  public ClientInfo[] GetClients()
    => clients.ToArray();

  public bool TryAdd(ClientInfo client)
  {
    if (clients.Count >= maxClient)
      return false;
    clients.Add(client);
    return true;
  }
  
  public async Task ClearClients()
  {
    while (clients.TryTake(out var client))
      await client.DisposeAsync();
  }
}