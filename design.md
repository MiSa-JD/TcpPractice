# 설계 해보기

## 러프

이번꺼의 가장 중요한 점은 아마 다음과 같을 것:

- 패킷의 구조 정의
- 바이트 스트림 해석기
  - 깨진 스트림 복원
  - 여러 메세지가 들어와도 분리
  - 불완전한 데이터는 다음 수신까지 보관
- 비정상적 패킷 쳐내기

이 요소들은 서버, 클라이언트 양측이 둘 다 갖고 있어야 하고, TCP 통신의 핵심임

## 패킷의 구조

어떤게 필요할까?

![alt text](https://velog.velcdn.com/images/user1/post/30b6ab09-8ca9-4df8-a3df-d610bc43aaea/image.png)
일단 일반적인 TCP 패킷은 다음과 같음

이걸 다 구현할 필요는 없음. 왜? 우리 패킷은 이 위에 올려질거라..

이건 네트워크 통신에서 쓰이는 구조일 뿐이고, 우리 어플리케이션에 맞는 패킷 구조를 따로 만들어야 함 ㅇㅇ 필요한 구조만 갖다 쓰면 됨

...

일단 최소한으로만 만들어보자

- Length: 4 bytes (32 bits) || Type: 2 bytes (16 bits) || Payload ||
  - Length: Payload 길이
    - uint32 범위
    - byte 단위로 계산함
      - 최대 2^32 - 1 bytes까지 가능
      - 하지만 악성 메세지 이슈가 있기에 크기 제한을 둬야 함 ㅇㅇ..
    - Big Endian
  - Type: 메세지 종류
    - Big Endian
    - Enum으로 정의해야할 것
      - 000: unknown
      - 1xx, 클라이언트 요청
        - 100: 일반 메세지 (broadcast) 요청
        - 101: /msg, 귓말 (unicast) 요청
        - 102: /stop 요청
        - 103: 서버 연결 요청
        - 104: /reload, 서버 정보 재요청
      
      - 2xx, 서버 응답
        - 200: 시스템 메세지 (서버 쪽에서 띄울 메세지, 이외의 상호작용은 없는 경우 사용)
        - 201: 연결 시 유저 정보 제공
        - 202: chat broadcast
        - 203: chat unicast
        - 204: 추가된 유저 정보 제공
        - 205: 종료된 유저 정보 제공
        - 206: 입장 허가, UUID 제공
        - 207: 입장 거절, 이유 제공
      
      - 3xx, 서버 요청
        - 300: ping

      - 4xx, 클라이언트 응답
        - 400: pong

      - 5xx, 게임에서의 클라이언트의 요청?
        - ...
      - 6xx, 게임에서의 서버 응답?
        - ...
      - ...
  - Payload: 실제 데이터
    - 최대 길이는 Length에 의해 정의됨
    - 규격은 Type에 따라 결정 됨

### Payload 구조 - Type 별 구조

> [!info] 참고 사항들
> - message들은 공통적으로 UTF-8 포멧을 따름
>   - 항상 명세의 마지막에 위치하며, (Length에서 정의된 크기 - 앞서 사용한 바이트 크기) 내의 동적인 크기를 갖음
> - 밑에 인자에 대한 설명이 없는 경우 Payload Length = 0
> - UUID는 문자열이 아닌 원시 16바이트를 제공해야 함
>   - 만약 문자열이 필요할 경우 클라이언트가 직접 변환하기 

#### 클라이언트 요청

- 100: 일반 메세지
  - Message
- 101: /msg
  - Receiver id: 16 bytes
  - Message
- 102: /stop
  - 일단 테스트 용, 나중에 관리자용으로 변경 필요
- 103: 서버 연결 요청
  - Name Length: 1 byte
  - Name: UTF-8

#### 서버 이벤트

- 200: 시스템 메세지
  - Level: 1 byte
    - 1: Info, 단순 정보 제공, 명령어 처리 결과 등
    - 2: Warn, 경고, 명령어 잘못됨 등
    - 3: Error, 에러 발생, 처리 실패 등
  - Message
- 201: 유저 리스트 제공
  - Count: 2 bytes
  - User 1: 
    - UUID: 16 bytes
    - Name Length: 1 byte
    - Name: UTF-8
  - User 2:
    - ...
  - ...
- 202: chat broadcast
  - Sender ID: 16 bytes
  - Message
- 203: chat unicast
  - Sender ID: 16 bytes
  - Message
- 204: 추가된 유저
  - UUID: 16 bytes
  - Name Length: 1 byte
  - Name: UTF-8
- 205: 종료된 유저
  - UUID: 16 bytes
- 206: 입장 허가
  - UUID: 16 bytes
- 207: 입장 거절
  - Reason: 1 byte
  - Message

#### 서버 요청
- 300: ping
  - Number: 4 bytes
  - TIMESTAMP: int64, 8 bytes
    - 전송 시각

#### 클라이언트 응답
- 400: pong
  - Number: 4 bytes
    - ping 에서 받은 번호 그대로 전송
  - TIMESTAMP: int64, 8 bytes
    - ping 에서 받은 시각 그대로 전송

### 명령어 규격

- /msg {유저명} {메세지}
  - 특정 유저에게만 메세지 제공
  - 101로 주기
- /stop
  - 서버 종료
  - 끄기 전에 200으로 서버 끈다고 알려주기
- /reload
  - 서버 정보 새로고침
  - 201로 주기

#### 예외 - 클라이언트가 직접 처리
- /exit
  - 클라이언트가 직접 연결 종료
- /list
  - 현재 (방)유저 목록