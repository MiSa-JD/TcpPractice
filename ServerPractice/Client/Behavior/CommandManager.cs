using Client.User;
using Protocol.Connection;
using Protocol.Packet.Payloads.ClientRequest;

namespace Client.Behavior;

public class CommandManager
{
  private CommandManager() { }
  public static CommandManager _instance { get; } = new();

  public async Task ProcessCommandInput(ConnectionInfo connection, string input)
  {
    string[] split = input.Split(' ');
    
    switch (split[0])
    {
      case "/msg":
        if (!UserMapper._instance.IsRoomUser(split[1]))
          return;
        string message = String.Join(" ", split[2..]);
        await connection.SendAsync(
          new UnicastRequestPayload(
            UserMapper._instance.GetUserByName(split[1]),
            message).ToPacket());
        break;
      case "/list":
        UserMapper._instance.PrintUserList();
        break;
      case "/reload":
        Console.WriteLine("Reload uwu");
        break;
      case "/leave":
        Console.WriteLine("Leave uwu");
        break;
      default:
        Console.WriteLine("UNKNOWN COMMAND!");
        break;
    }
  }
}