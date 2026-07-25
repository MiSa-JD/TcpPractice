### 100: BroadcastRequest
> 같은 방에 있는 유저에게 전체 메세지
- string message

### 101: UnicastRequest
> 방에 있는 유저 한 명에게 귓속말
- Guid receiver
- string message

### 102: StopRequest
> 서버 끄기

### 103: DisconnectingRequest
> 클라이언트 연결 끊기

### 104: RoomListRequest
> 방 목록 요청

### 105: RoomEntranceRequest
> 방 입장 요청
- int roomid

### 106: RoomExitRequest
> 방 퇴장 요청