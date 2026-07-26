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
	- string title
	- int curUserCount
- room2: 
	- int roomNum
	- string title
	- int curUserCount
- ...

### 202: BroadcastEvent
> 다른 유저에게 요청 받은 전체 메세지 전달
>> 구분자는 roomUserId
- int nameLength
- int senderId
- int messageLength
- string message
	- UTF-16

### 203: UnicastEvent
> 다른 유저에게 요청 받은 귓속말 전달
>> 구분자는 roomUserId
- int senderId
- string message
	- UTF-16

### 204: AddedUserInfoEvent
> 현재 방에 유저가 추가됨을 알림
- int roomUserId
- string username

### 205: ExitedUserInfoEvent
> 현재 방에 유저가 나감을 알림
- int roomUserId
- string username

### 206: ApplyEntranceResponse
> 방에 들어오기를 허가함, 방에 있는 유저 목록 제공
- int count
- User1:
	- int roomUserId
	- string username
- User2:
	- int roomUserId
	- string username
- ...
### 207: DenyEntranceResponse
> 방에 들어오기를 거절함

### 208: 

### 209: RoomListResponse
> 방 목록 제공
- int roomCount
- room1:
	- int roomNum
	- string title
	- int curUserCount
- room2: 
	- int roomNum
	- string title
	- int curUserCount
- ...
### 210: UserListResponse
> 방에 있는 유저 목록 제공
- int count
- User1:
	- int roomUserId
	- string username
- User2:
	- int roomUserId
	- string username
- ...
### 211:  AddRoomEvent
> 방이 추가됨을 알림
- int roomNum
- string title
### 212: RemoveRoomEvent
> 방이 제거됨을 알림
- int roomNum
- string title