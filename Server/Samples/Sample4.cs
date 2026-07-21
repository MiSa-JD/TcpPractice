using System.Buffers.Binary;
using System.Collections;
using System.Text;
using Server.Types;

namespace Server;

enum PacketType
{
  // 클라이언트 요청
  BroadcastRequest = 100,
  UnicastRequest = 101,
  StopRequest = 102,
  EnterRequest = 103,
  ReloadRequest = 104,
  
  // 서버 응답 / 이벤트
  SystemMessage = 200,
  UserListInfo = 201,
  BroadcastResponse = 202,
  UnicastResponse = 203,
  AddedUserInfo = 204,
  RemovedUserInfo = 205,
  ApplyEntrance = 206,
  DenyEntrance = 207,
  
  // 서버 요청
  Ping = 300,
  
  // 클라이언트 응답
  Pong = 400,
}

class PacketInfo(Int16 type, byte[] payload)
{
  private const int headerSize = 4;
  public PacketType PacketType { get; } = (PacketType)type;
  public byte[] payload { get; } = payload;

  public static PacketInfo LoadPacket(byte[] buffer)
  {
    var tmp = buffer;
    
    if (buffer.Length < headerSize)
      throw new Exception("Not enough header bytes.");
    
    // BitConverter는 컴퓨터마다 BigEndian으로 변환할 수도, LittleEndian으로 변환할 수도 있음
    // BitConverter -> BinaryPrimitives.Read/*타입*/BigEndian()으로 사용하기
    
    // byte[..] 또한 복사를 일으키기에
    // 읽기 전용으로만 참조하는 .AsSpan(start, length) 로 처리하는게 낫다 함
    
    // int length = BitConverter.ToInt32(tmp[..4].ToArray());
    int length = BinaryPrimitives.ReadInt32BigEndian(tmp.AsSpan(0,4));
    
    if (length < 0)
      throw new Exception("Wrong packet lenght.");

    if (tmp.Length < 4 + 2 + length)
      throw new Exception("Packet buffer is too small.");

    // return new PacketInfo (
    //   type: BitConverter.ToInt16(tmp[4..6].ToArray()),
    //   payload: tmp[6..(6 + length)].ToArray());
    return new PacketInfo(
      type: BinaryPrimitives.ReadInt16BigEndian(tmp.AsSpan(4,2)),
      payload: tmp.AsSpan(headerSize).ToArray());
  }

  public static byte[] PacketBufferBuilder(short type, ReadOnlySpan<byte> payload)
  {
    // int length = payload.Length;
    // Queue<byte> queue = new Queue<byte>();
    // foreach (byte b in BitConverter.GetBytes(length))
    //   queue.Enqueue(b);
    // foreach (byte b in BitConverter.GetBytes(type))
    //   queue.Enqueue(b);
    // foreach (byte b in payload)
    //   queue.Enqueue(b);
    //
    // return queue.ToArray();
    byte[] buffer = new byte[headerSize + payload.Length];
    
    BinaryPrimitives.WriteInt32BigEndian(buffer,
      payload.Length);
    BinaryPrimitives.WriteInt16BigEndian(buffer,
      type);
    payload.CopyTo(buffer.AsSpan(headerSize));
    
    return buffer;
  }
}

public class Sample4
{
  public static void Main(String[] args)
  {
    Sample4 server = new();
    // PacketInfo 테스트
    short type = 100;
    string message = "hello?";
    // int length = Encoding.Unicode.GetByteCount(message); // 10

    byte[] m2b = Encoding.Unicode.GetBytes(message);

    // Console.WriteLine("enqueued buffer: " + BitConverter.ToString(buffer));
    
    PacketInfo packet = PacketInfo.LoadPacket(
      PacketInfo.PacketBufferBuilder(type, m2b));
    
    Console.WriteLine("Payload Length: " + packet.payload.Length);
    Console.WriteLine("Type: " + packet.PacketType);
    Console.WriteLine("Payload: " + BitConverter.ToString(packet.payload));
    Console.WriteLine("Payload to string: " +  Encoding.Unicode.GetString(packet.payload));
  }
}