namespace Server;

public class CommandManager
{
  private CommandManager () { }
  
  private static CommandManager? _manager;

  public static CommandManager GetManager () { return _manager ??= new CommandManager(); }
  
  public async Task ProcessCommand (ClientInfo client, string message)
  {
    
  }
}