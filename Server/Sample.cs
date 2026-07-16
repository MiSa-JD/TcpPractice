using System.Net;
using System.Net.Sockets;
using System.Text;

const int port = 50000;

// IPAddress.Loopback: 127.0.0.1
TcpListener listener = new TcpListener(IPAddress.Loopback, port);

listener.Start();
Console.WriteLine("Listening on port " + port);

using TcpClient client = await listener.AcceptTcpClientAsync(); // client를 선언, TCP 연결 요청이 오기까지 대기

Console.WriteLine("클라이언트 접속 완료");

// client에서 스트림을 받아옴
// 이곳에서 통신이 이뤄질 예정
// 반환값은 NetworkStream (Rider에서는 var을 추천해줌. js처럼 간단히 처리하고 싶을 때 사용하면 될 듯?)
await using NetworkStream stream = client.GetStream();

// TCP는 단순한 바이트 스트림임
// 따라서 단위 통신을 하려면 해당 단위를 정의해야 함
// 예시에서는 Line 단위 통신을 하려고 함

using StreamReader reader = new StreamReader(
  stream,
  new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
  leaveOpen: true);

// 이게 ㅅㅂ 뭔 문법이냐
await using StreamWriter writer = new(
  stream,
  // 밑에 이거 Python, JS에서 보던 명시적 인자 그거임 ㅇㅇ 
  // 인자이름: 값 구조
  new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
  leaveOpen: true)
// 밑에 이거 지우고 writer.AutoFlush = true 해도 같은 문법임.
// C# 문법 학습용이니 냅두셈 ㅇㅇ 
{
  AutoFlush = true
};

string? message = await reader.ReadLineAsync();
Console.WriteLine($"클라이언트가 보낸 메세지: {message}");

await writer.WriteLineAsync($"서버에서 온 메세지: {message}");

Console.WriteLine("응답 전송 완료");

listener.Stop();