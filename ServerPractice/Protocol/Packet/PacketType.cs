namespace Protocol.Packet;

public enum PacketType
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