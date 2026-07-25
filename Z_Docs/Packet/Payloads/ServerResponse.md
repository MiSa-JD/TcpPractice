### 200: SystemMessageEvent
> 시스템 메세지
- char level
	- 1: Info
	- 2: Warn
	- 3: Error
- string message

### 201: ApplyConnectionResponse
> TCP 파이프 연결 완료, UUID 제공, 방 목록 제공
- Guid uuid
- int roomCount
- room1:
	- int roomNum
	- int titleLength
	- string title
	- int curUserCount
	- int maxUserCount
- room2: 
	- int roomNum
	- int titleLength
	- string title
	- int curUserCount
	- int maxUserCount
- ...

### 202: BroadcastEvent
> 다른 유저에게 요청 받은 전체 메세지 전달
- string username
	- UTF-8
	- 32byte
- string message
	- UTF-16

### 203: UnicastEvent
> 다른 유저에게 요청 받은 귓속말 전달
- string username
	- UTF-8
	- 32byte
- string message
	- UTF-16

### 204: AddedUserInfoEvent
> 현재 방에 유저가 추가 됨을 알림
- string username

### 205: ExitedUserInfoEvent
> 현재 방에 유저가 나감을 알림
- string username

### 206: ApplyEntranceResponse
> 방에 들어오기를 허가함, 방에 있는 유저 목록 제공
- int count
- User1:
	- char nameLength
	- string username
- User2:
	- char nameLength
	- string username
- ...
### 207: DenyEntranceResponse
> 방에 들어오기를 거절함, 이유 제공
- string message

### 208: ConnectionFailedResponse
> 알 수 없는 이유로 연결 실패함

### 209: RoomListResponse
> 방 목록 제공

### 210: UserListResponse
> 방에 있는 유저 목록 제공

### 211:  AddRoomEvent
> 방이 추가됨을 알림

### 212: RemoveRoomEvent
> 방이 제거됨을 알림
