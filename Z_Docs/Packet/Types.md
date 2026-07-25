# 타입 나열
> [!info] 참고 사항들
> - Message들은 공통적으로 UTF-16(Unicode) 포멧을 따름
>   - 항상 명세의 마지막에 위치하며, (Length에서 정의된 크기 - 앞서 사용한 바이트 크기) 내의 동적인 크기를 갖음
>   - Message를 제외한 문자열 데이터는 항상 UTF-8로 처리하기
>     - 얘넨 영문+부호만으로 처리할 가능성이 높기에
> - 밑에 인자에 대한 설명이 없는 경우 Payload Length = 0
> - UUID는 문자열이 아닌 원시 16바이트를 제공해야 함
>   - 만약 문자열이 필요할 경우 클라이언트가 직접 변환하기 

## 0xx: 미정

## 1xx: 클라이언트의 요청
```dataviewjs
const path = "1. Project/포트폴리오 작성기/TCP 연습/Packet/Payloads/ClientRequest.md";
const content = await dv.io.load(path);

// 코드 블록 안의 ###은 제외
const text = content.replace(/```[\s\S]*?```/g, "");

const headings = [...text.matchAll(/^###(?!#)\s+(.+?)\s*#*\s*$/gm)]
    .map(match => match[1].trim());

dv.list(
    headings.map(heading =>
        dv.sectionLink(path, heading, false, heading)
    )
);
```
## 2xx: 서버의 응답 및 이벤트
```dataviewjs
const path = "1. Project/포트폴리오 작성기/TCP 연습/Packet/Payloads/ServerResponse.md";
const content = await dv.io.load(path);

// 코드 블록 안의 ###은 제외
const text = content.replace(/```[\s\S]*?```/g, "");

const headings = [...text.matchAll(/^###(?!#)\s+(.+?)\s*#*\s*$/gm)]
    .map(match => match[1].trim());

dv.list(
    headings.map(heading =>
        dv.sectionLink(path, heading, false, heading)
    )
);
```

## 3xx: 서버의 요청
```dataviewjs
const path = "1. Project/포트폴리오 작성기/TCP 연습/Packet/Payloads/ServerRequest.md";
const content = await dv.io.load(path);

// 코드 블록 안의 ###은 제외
const text = content.replace(/```[\s\S]*?```/g, "");

const headings = [...text.matchAll(/^###(?!#)\s+(.+?)\s*#*\s*$/gm)]
    .map(match => match[1].trim());

dv.list(
    headings.map(heading =>
        dv.sectionLink(path, heading, false, heading)
    )
);
```

## 4xx: 클라이언트의 응답
```dataviewjs
const path = "1. Project/포트폴리오 작성기/TCP 연습/Packet/Payloads/ClientResponse.md";
const content = await dv.io.load(path);

// 코드 블록 안의 ###은 제외
const text = content.replace(/```[\s\S]*?```/g, "");

const headings = [...text.matchAll(/^###(?!#)\s+(.+?)\s*#*\s*$/gm)]
    .map(match => match[1].trim());

dv.list(
    headings.map(heading =>
        dv.sectionLink(path, heading, false, heading)
    )
);
```

## 5xx: 게임에서의 클라이언트의 요청

## 6xx: 게임에서의 서버의 응답 및 이벤트