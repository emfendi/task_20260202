# Employee Contact API

직원 긴급 연락망 관리 API - .NET 10 (C#) 구현

## 기술 스택

- **.NET 10** (C#)
- **Entity Framework Core** (In-Memory Database)
- **Serilog** (구조화된 로깅)
- **Swashbuckle** (OpenAPI/Swagger)
- **xUnit** (단위 테스트)
- **Clean Architecture** + **CQRS 패턴**

## 프로젝트 구조

```
├── Domain/                        # 도메인 레이어
│   ├── Models/                   # Employee 도메인 모델
│   ├── ValueObjects/             # PhoneNumber 값 객체
│   └── Exceptions/               # 커스텀 예외 (DuplicateEmail, InvalidDataFormat)
├── Application/                   # 애플리케이션 레이어
│   ├── Commands/                 # CQRS Commands
│   │   ├── Dto/                 # CreateEmployeeCommand
│   │   └── Handlers/            # ICreateEmployeeCommandHandler
│   ├── Queries/                  # CQRS Queries
│   │   ├── Dto/                 # Response DTOs
│   │   └── Handlers/            # IEmployeeQueryHandler
│   └── Services/                 # Parser 서비스 (CSV/JSON)
├── Infrastructure/                # 인프라 레이어
│   ├── Entities/                 # EF Core 엔티티
│   └── Persistence/              # Repository 구현 (Domain 모델 사용)
├── Presentation/                  # 프레젠테이션 레이어
│   ├── Controllers/              # API 컨트롤러
│   ├── Dto/                      # Request/Response DTO
│   ├── Middleware/               # 예외 처리, 포맷터
│   └── Swagger/                  # Swagger 커스텀 필터
└── Tests/                         # 단위 테스트
    └── EmployeeContactApi.Tests/ # xUnit 테스트 프로젝트
```

## 빌드 및 실행

### 필수 요구사항
- .NET 8.0 SDK 이상

### 빌드
```bash
dotnet build
```

### 실행
```bash
dotnet run
```

애플리케이션이 시작되면 http://localhost:5255 에서 접근 가능합니다.

### 테스트 실행
```bash
cd Tests
dotnet test
```

### Swagger UI
http://localhost:5255/ 에서 API 문서를 확인할 수 있습니다.

## API 엔드포인트

### 1. 직원 목록 조회 (페이징)
```
GET /api/employee?page={page}&pageSize={pageSize}
```

**응답:** 200 OK
```json
{
  "content": [
    {"id": 1, "name": "김철수", "email": "charles@example.com", "tel": "01075312468", "joined": "2018-03-07"}
  ],
  "page": 0,
  "pageSize": 10,
  "totalElements": 1,
  "totalPages": 1
}
```

### 2. 이름으로 직원 조회
```
GET /api/employee/{name}
```

**응답:** 200 OK
```json
[
  {"id": 1, "name": "김철수", "email": "charles@example.com", "tel": "01075312468", "joined": "2018-03-07"}
]
```

### 3. 직원 등록
```
POST /api/employee
```

**지원 Content-Type:**
- `application/json` - JSON body
- `text/csv` - CSV body
- `multipart/form-data` - 파일 업로드

#### JSON Body
```bash
curl -X POST http://localhost:5255/api/employee \
  -H "Content-Type: application/json" \
  -d '[{"name":"김철수","email":"charles@example.com","tel":"01075312468","joined":"2018-03-07"}]'
```

#### CSV Body
```bash
curl -X POST http://localhost:5255/api/employee \
  -H "Content-Type: text/csv" \
  -d '김철수, charles@example.com, 01075312468, 2018.03.07'
```

#### 파일 업로드
```bash
curl -X POST http://localhost:5255/api/employee \
  -F "file=@employees.csv"
```

**응답:** 201 Created
```json
{"count": 3}
```

**에러 응답:** 409 Conflict (이메일 중복 시)
```json
{"status": 409, "error": "Conflict", "message": "Duplicate email(s) found: charles@example.com"}
```

## 지원 데이터 형식

### CSV 형식
```csv
김철수, charles@example.com, 01075312468, 2018.03.07
박영희, matilda@example.com, 01087654321, 2021.04.28
```

### JSON 형식
```json
[
  {"name":"김클로", "email":"clo@example.com", "tel":"010-1111-2424", "joined":"2012-01-05"},
  {"name":"박마블", "email":"md@example.com", "tel":"010-3535-7979", "joined":"2013-07-01"}
]
```

### 지원 날짜 형식
- `yyyy.MM.dd` (예: 2018.03.07)
- `yyyy-MM-dd` (예: 2018-03-07)
- `yyyy/MM/dd` (예: 2018/03/07)

### 전화번호 형식
- 10-11자리 숫자 (01로 시작)
- 하이픈 자동 제거 (010-1234-5678 → 01012345678)

## 설계 특징

### Clean Architecture
- **Domain**: 비즈니스 모델, Value Object, 예외
- **Application**: 유스케이스 (Command/Query Handler)
- **Infrastructure**: 데이터 접근 (Repository)
- **Presentation**: API 엔드포인트

### CQRS 패턴
- **Command**: 데이터 변경 작업 (직원 등록)
- **Query**: 데이터 조회 작업 (직원 목록, 이름 검색)
- Handler 인터페이스로 의존성 역전 (DIP)
- Repository도 Command/Query로 분리

### 비즈니스 규칙
- 이메일 중복 검사 (Application 레이어)
- 전화번호 정규화 (Domain Value Object)
- 데이터 유효성 검증 (이름, 이메일, 전화번호, 날짜)

### 로깅
- Serilog를 사용한 구조화된 로깅
- 컨트롤러, 핸들러, 리포지토리 레벨에서 로깅

### 예외 처리
- 전역 예외 핸들러로 일관된 에러 응답
- `400 Bad Request`: 유효성 검증 오류, 데이터 형식 오류
- `409 Conflict`: 이메일 중복

### 테스트
- xUnit 기반 단위 테스트 (16개)
- Parser 테스트 (CSV, JSON)
- DateParser 테스트

## 테스트 데이터

`testdata/` 폴더에 예시 파일이 있습니다:
- `employees.json` - JSON 형식 샘플
- `employees.csv` - CSV 형식 샘플
