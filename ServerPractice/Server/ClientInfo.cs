using System.Net.Sockets;
using System.Threading.Channels;
using Protocol.Packet;

namespace Server;

public class ClientInfo: IAsyncDisposable
{
  private readonly Stream stream;
  private readonly TcpClient client;
  private readonly Channel<PacketInfo> channel = Channel.CreateBounded<PacketInfo>(
    new BoundedChannelOptions(32)
    {
      SingleReader = true,
      SingleWriter = true,
      FullMode = BoundedChannelFullMode.DropOldest
    });
  private readonly Task sendingTask;
  
  public ClientInfo(TcpClient client, CancellationToken token)
  {
    this.client = client;
    stream = this.client.GetStream();
    
    sendingTask = RunSender(token);
  }

  // 패킷을 전송 큐에 저장
  public async Task SendAsync(PacketInfo packet)
  {
    await channel.Writer.WriteAsync(packet);
  }

  public async Task SendAsync(byte[] bytes)
  {
    await channel.Writer.WriteAsync(PacketCodec.GetManager().Bytes2Packet(bytes));
  }

  public async Task SendAsync(PacketType type, byte[] payload)
  {
    await SendAsync(
      PacketCodec.GetManager()
        .Data2Packet(type, payload));
  }

  // 클라이언트 생성 시 
  private async Task RunSender(CancellationToken token)
  {
    await foreach (var packet in channel.Reader.ReadAllAsync(token))
    {
      await stream.WriteAsync(
        PacketCodec
          .GetManager()
          .Packet2Bytes(packet)
        , token);
    }
  }

  public async ValueTask DisposeAsync()
  {
    channel.Writer.Complete();
    try
    {
      await sendingTask;
    }
    catch (OperationCanceledException) { }
    catch (IOException) { }
    finally
    {
      await stream.DisposeAsync();
      client.Dispose();
    }
  }
}