namespace Protocol.Packet.Models;

public enum PacketType: short
{
  // 클라이언트 요청
  BroadcastRequest = 100,
  UnicastRequest = 101,
  StopRequest = 102, // x
  DisconnectingRequest = 103,
  RoomListRequest = 104,
  RoomEntranceRequest = 105,
  RoomExitRequest = 106,
  

  // 서버 응답 / 이벤트
  SystemMessageEvent = 200,
  ApplyConnectionResponse = 201,
  BroadcastEvent = 202,
  UnicastEvent = 203,
  AddedUserInfoEvent = 204,
  ExitedUserInfoEvent = 205,
  ApplyEntranceResponse = 206,
  DenyEntranceResponse = 207,
  RoomListResponse = 209,
  UserListResponse = 210,
  AddRoomEvent = 211,
  RemoveRoomEvent = 212,

  // 서버 요청
  Ping = 300,
  UsernameRequest = 301,

  // 클라이언트 응답
  Pong = 400,
  UsernameResponse = 401,
}