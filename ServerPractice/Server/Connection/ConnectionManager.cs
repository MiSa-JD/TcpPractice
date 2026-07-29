// using System.Collections.Concurrent;
// using Protocol.Connection;
//
// namespace Server.Connection;
//
// public class ConnectionManager
// {
//   private ConnectionManager() { }
//   private const int maxClient = 5;
//   public static ConnectionManager _instance { get; } = new();
//   private static readonly ConcurrentBag<ConnectionInfo> clients = [];
//   
//   private readonly Lock addLock = new();
//   public int GetCount ()
//     => clients.Count;
//   
//   public ConnectionInfo[] GetClients()
//     => clients.ToArray();
//
//   public bool TryAdd(ConnectionInfo connection)
//   {
//     lock (addLock)
//     {
//       if (clients.Count >= maxClient)
//         return false;
//       clients.Add(connection);
//       return true;
//     }
//   }
//   
//   public async Task ClearClients()
//   {
//     while (clients.TryTake(out var client))
//       await client.DisposeAsync();
//   }
// }