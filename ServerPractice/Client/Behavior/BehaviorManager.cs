using System.Threading.Channels;
using Client.User;
using Protocol.Connection;
using Protocol.Packet.Models;
using Protocol.Packet.Payloads.ClientRequest;
using Protocol.Packet.Payloads.ClientResponse;
using Protocol.Packet.Payloads.ServerResponseEvent;
using Protocol.SystemMessage;
using Protocol.Token;
using BroadcastEventPayload = Protocol.Packet.Payloads.ServerResponseEvent.BroadcastEventPayload;

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
          var packet = new UsernameResponsePayload(input)
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
      var packet = await connection.ReceiveAsync(TokenManager._instance.token);
      if (packet is null)
      {
        Console.WriteLine("Connection closed. Terminating...");
        return;
      }

      Console.WriteLine("Received Packet: " + packet.type);
      switch (packet.type)
      {
        case PacketType.SystemMessageEvent: // 200
          // Console.WriteLine("SYSTEM MESSAGE");
          await enqueue(ProcessSystemMessage(packet.payload));
          break;
        case PacketType.ApplyConnectionResponse: // 201
          await enqueue(ProcessApplyConnectionResponse(packet.payload));
          break;
        case PacketType.BroadcastEvent: // 202
          await enqueue(ProcessBroadcastEvent(packet.payload));
          break;
        case PacketType.UnicastEvent:
          await enqueue(ProcessUnicastEvent(packet.payload));
          break;
        case PacketType.UsernameRequest: // 301
          // Console.WriteLine("USERNAME REQUEST");
          await enqueue(ProcessUsernameRequest());
          break;
        case PacketType.ApplyEntranceResponse:
        case PacketType.UserListResponse:
          await enqueue(ProcessUserListResponse(packet.payload));
          break;
        case PacketType.AddedUserInfoEvent:
          await enqueue(ProcessAddedUserInfoEvent(packet.payload));
          break;
        case PacketType.ExitedUserInfoEvent:
          await enqueue(ProcessExitedUserInfoEvent(packet.payload));
          break;
        default:
          Console.WriteLine("UNKNOWN PACKET");
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
    {
      await CommandManager._instance.ProcessCommandInput(connection, input);
      return;
    }

    await connection.SendAsync(new BroadcastRequestPayload(input).ToPacket());
  }

  private async Task ProcessApplyConnectionResponse(byte[] bytes)
  {
    ApplyConnectionResponsePayload.ReadBytes(bytes, out var _body);
    var body = (ApplyConnectionResponsePayload)_body;
    
    Console.WriteLine("내 uuid: " + body.uuid);
    Console.WriteLine("현재 방 개수: " + body.rooms.Count);
    foreach (var room in body.rooms)
    {
      Console.WriteLine(" 방 이름: " + room.title);
      Console.WriteLine("  방 코드: " + room.roomId);
      Console.WriteLine("  사람 수: " + room.curUserCount);
    }
  }

  public async Task ProcessUserListResponse(byte[] bytes)
  {
    UserListResponsePayload.ReadBytes(bytes, out var _body);
    var body = (UserListResponsePayload)_body;
    await UserMapper._instance.InitializeUser(body.users);
  }

  public async Task ProcessBroadcastEvent(byte[] bytes)
  {
    BroadcastEventPayload.ReadBytes(bytes, out var _body);
    var body = (BroadcastEventPayload)_body;
    Console.WriteLine(body.username + ": " + body.message);
  }

  public async Task ProcessUnicastEvent(byte[] bytes)
  {
    UnicastEventPayload.ReadBytes(bytes, out var _body);
    var body = (UnicastEventPayload)_body;
    Console.WriteLine(body.senderName + ": " + body.message);
  }

  public async Task ProcessAddedUserInfoEvent(byte[] bytes)
  {
    AddedUserInfoEventPayload.ReadBytes(bytes, out var _body);
    var body = (AddedUserInfoEventPayload)_body;
    await UserMapper._instance.AddUser(body.roomUserId, body.username);
  }
  
  private async Task ProcessExitedUserInfoEvent(byte[] packetPayload)
  {
    AddedUserInfoEventPayload.ReadBytes(packetPayload, out var _body);
    var body = (AddedUserInfoEventPayload)_body;
    await UserMapper._instance.RemoveUser(body.roomUserId);
  }

  public ValueTask DisposeAsync()
  {
    runTask.Dispose();
    return ValueTask.CompletedTask;
  }
}