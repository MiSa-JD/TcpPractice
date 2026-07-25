using System.Threading.Channels;
using Protocol.Connection;
using Protocol.Packet.Models;
using Protocol.Packet.Payloads.ClientRequest;
using Protocol.Packet.Payloads.ClientResponse;
using Protocol.Packet.Payloads.ServerResponseEvent;
using Protocol.SystemMessage;
using Protocol.Token;

namespace Client.Behavior;

public class BehaviorManager: IAsyncDisposable
{
  private BehaviorManager() { runTask = Run(); }
  public static BehaviorManager _instance { get; } = new();
  private readonly Channel<Task> taskChannel = Channel.CreateUnbounded<Task>();
  private readonly Task runTask;
  private readonly CancellationToken token = TokenManager._instance.token;
  private InputStatus inputStatus = InputStatus.lobby;
  private ValueTask enqueue(Task task)
    => taskChannel.Writer.WriteAsync(task, token);

  private async Task Run()
  {
    await foreach(var task in taskChannel.Reader.ReadAllAsync(token))
    {
      await task;
    }
  }
 
  public async Task GetUserInput(ConnectionInfo connection)
  {
    while (!TokenManager._instance.token.IsCancellationRequested)
    {
      var input = await Console.In.ReadLineAsync(token);
      if (input is null or "") continue;
      switch (inputStatus)
      {
        case InputStatus.chatting:
          await ProcessDefaultUserInput(connection, input);
          break;
        case InputStatus.nameReq:
          var packet = new UsernameResponse(input)
            .ToPacket();
          await connection.SendAsync(packet);
          inputStatus = InputStatus.chatting;
          break;
        case InputStatus.lobby:
          break;
      }
    }
  }
  public async Task ReceivePackets(ConnectionInfo connection)
  {
    Console.WriteLine("Listening...");
    while (!TokenManager._instance.token.IsCancellationRequested)
    {
      PacketInfo packet = await connection.ReceiveAsync(TokenManager._instance.token);
      
      Console.WriteLine("Received Packet: " + packet.type);
      switch (packet.type)
      {
        case PacketType.SystemMessage: // 200
          // Console.WriteLine("SYSTEM MESSAGE");
          await enqueue(ProcessSystemMessage(packet.payload));
          break;
        case PacketType.UsernameRequest: // 301
          // Console.WriteLine("USERNAME REQUEST");
          await enqueue(ProcessUsernameRequest());
          break;
        default:
          // Console.WriteLine("UNKNOWN PACKET");
          break;
      }
    }
  }

  private Task ProcessSystemMessage(byte[] payload)
  {
    SystemMessagePayload.ReadBytes(payload, out var body);
    SystemMessagePayload tmp = (SystemMessagePayload)body;
    switch (tmp.level)
    {
      case SystemMessageType.Info:
        Console.WriteLine("==INFO: " + tmp.message);
        break;
      case SystemMessageType.Warn:
        Console.WriteLine("==!WARN: " + tmp.message + "!!");
        break;
      case SystemMessageType.Error:
        Console.WriteLine("==!!ERROR: " + tmp.message + "!!");
        break;
    }
    
    return Task.CompletedTask;
  }

  private Task ProcessUsernameRequest()
  {
    inputStatus = InputStatus.nameReq;
    Console.Write("Enter your name: ");
    return Task.CompletedTask;
  }

  private async Task ProcessDefaultUserInput(ConnectionInfo connection, string input)
  {
    // 명령어 처리
    if (input.ToCharArray()[0] == '/')
      return;
    await connection.SendAsync(new BroadcastRequestPayload(input).ToPacket());
  }

  public ValueTask DisposeAsync()
  {
    runTask.Dispose();
    return ValueTask.CompletedTask;
  }
}