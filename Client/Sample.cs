using System.Net;using System.Net.Sockets;
using System.Text;

const int port = 50000;
const string address = "127.0.0.1";

// 연결되지 않은 클라이언트 객체
using TcpClient client = new();

// 연결 중
Console.WriteLine($"Connecting to {address}:{port}...");
await client.ConnectAsync(address, port);

// 연결 완료 시
Console.WriteLine($"Connected to {address}:{port}");

await using NetworkStream stream = client.GetStream();

using StreamReader reader = new(
  stream,
  Encoding.UTF8,
  detectEncodingFromByteOrderMarks: false,
  leaveOpen: true);
  
await using StreamWriter writer = new(
  stream,
  new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
  leaveOpen: true)
{
  AutoFlush = true
};

await writer.WriteLineAsync("Hello World!");

string? response = await reader.ReadLineAsync();

Console.WriteLine(response);