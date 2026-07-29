using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading.Channels;
using Protocol.Packet;
using Protocol.Packet.Models;

namespace Protocol.Connection;

public class ConnectionInfo: IAsyncDisposable
{
  private readonly Stream stream;
  private readonly TcpClient client;
  private readonly Channel<PacketInfo> channel = Channel.CreateBounded<PacketInfo>(
    new BoundedChannelOptions(32)
    {
      SingleReader = true,
      SingleWriter = false,
      FullMode = BoundedChannelFullMode.Wait
    });
  public const int maxPacketLength = 4096;
  public int connectionId;
  private readonly Task sendingTask;
  
  public ConnectionInfo(TcpClient client, CancellationToken token)
  {
    this.client = client;
    stream = this.client.GetStream();
    
    sendingTask = RunSender(token);
  }
  
  // 큐에 담긴 메세지 실제로 보내기
  private async Task RunSender(CancellationToken token)
  {
    await foreach (var packet in channel.Reader.ReadAllAsync(token))
    {
      await stream.WriteAsync(
        PacketCodec
          .Packet2Bytes(packet)
        , token);
    }
  }

  // 패킷을 전송 큐에 저장
  public async Task SendAsync(PacketInfo packet)
  {
    if (packet.payload.Length > maxPacketLength)
    {
      Console.WriteLine("Too long packets!");
      return;
    }

    await channel.Writer.WriteAsync(packet);
  }

  public async Task<PacketInfo?> ReceiveAsync(CancellationToken token)
  {
    byte[] buffer = new byte[1];
    int readCount = await stream.ReadAsync(buffer, token);
    if (readCount == 0)
    {
      return null;
    }

    if (buffer[0] != PacketCodec.MagicByte)
      throw new Exception("Invalid Magic Byte!");
    
    buffer = new byte[4];
    await stream.ReadExactlyAsync(buffer, token);
    int length = BinaryPrimitives.ReadInt32BigEndian(buffer);

    if (length < 0)
    {
      Console.WriteLine("Wrong Packet Length!");
      return null;
    }
    if (length > maxPacketLength)
    {
      Console.WriteLine("Too Long Packet!");
      return null;
    }
    buffer = new byte[2];
    await stream.ReadExactlyAsync(buffer, token);
    PacketType type = (PacketType)BinaryPrimitives.ReadInt16BigEndian(buffer);
    
    buffer = new byte[length];
    await stream.ReadExactlyAsync(buffer, token);
    // Console.WriteLine("Read Payload: " + BitConverter.ToString(buffer));
    return new PacketInfo(type, buffer);
  }

  public string GetAddress()
  {
    return client.Client.RemoteEndPoint!.ToString()!;
  }
  // 무조건 PacketInfo로 보내도록 막아두는게 나으려나?
  // public async Task SendAsync(byte[] bytes)
  // {
  //   await channel.Writer.WriteAsync(PacketCodec.GetManager().Bytes2Packet(bytes));
  // }
  //
  // public async Task SendAsync(PacketType type, byte[] payload)
  // {
  //   await SendAsync(
  //     PacketCodec.GetManager()
  //       .Data2Packet(type, payload));
  // }
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