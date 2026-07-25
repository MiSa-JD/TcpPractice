namespace Protocol.Packet.Models;

public enum PacketType: short
{
  // 클라이언트 요청
  BroadcastRequest = 100,
  UnicastRequest = 101,
  StopRequest = 102, // x
  ExitRequest = 103,
  RoomListRequest = 104,
  RoomEntranceRequest = 105,

  // 서버 응답 / 이벤트
  SystemMessage = 200,
  ApplyConnectionResponse = 201,
  BroadcastResponse = 202,
  UnicastResponse = 203,
  AddedUserInfoEvent = 204,
  RemovedUserInfo = 205,
  ApplyEntrance = 206,
  DenyEntrance = 207,

  // 서버 요청
  Ping = 300,
  UsernameRequest = 301,

  // 클라이언트 응답
  Pong = 400,
  UsernameResponse = 401,
}