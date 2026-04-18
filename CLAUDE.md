# AgendeX — Project Context for Claude Code

## Overview

Web system for managing service appointments between clients and agents/specialists. Developed as a practical assessment for the SENAI/FIESC hiring process (01064/2026).

## Stack

### Backend
- .NET 10 — ASP.NET Core Web API
- Clean Architecture (Domain / Application / Infrastructure / WebAPI)
- CQRS with MediatR
- Entity Framework Core + PostgreSQL
- FluentValidation
- AutoMapper
- JWT Bearer Authentication
- Swagger/OpenAPI (Swashbuckle)
- ClosedXML (XLSX export)
- xUnit + Moq + FluentAssertions (tests)

### Frontend
- React 18 + TypeScript + Vite
- React Query (TanStack Query)
- React Hook Form + Zod
- Axios
- Tailwind CSS + shadcn/ui
- Zustand

### Infrastructure
- Docker + Docker Compose
- PostgreSQL 16

## Folder Structure

```
prova-dotnet-react-senior-01064-2026/
├── backend/
│   ├── AgendeX.slnx
│   ├── AgendeX.Domain/              # Entities, enums, interfaces
│   ├── AgendeX.Application/         # Features (Commands/Queries) + Common (Behaviors/Interfaces)
│   ├── AgendeX.Infrastructure/      # Persistence (EF/repositories/migrations), Services, Identity
│   ├── AgendeX.WebAPI/              # Controllers, Swagger, middlewares, Program.cs
│   ├── AgendeX.Tests/               # Unit tests (xUnit + Moq)
│   └── scripts/                     # Utility scripts (seed, etc.)
├── frontend/
│   └── src/
│       ├── components/              # Reusable components
│       ├── pages/                   # Module-based pages
│       ├── hooks/                   # Custom hooks
│       ├── services/                # API calls (Axios)
│       ├── store/                   # Global state (Zustand)
│       ├── types/                   # TypeScript interfaces
│       └── utils/                   # Helpers and formatters
├── docker-compose.yml
└── README.md
```

## Domain Model

### Main Entities

```
User
  - Id (Guid)
  - Name (string)
  - Email (string, unique)
  - PasswordHash (string)
  - Role (enum: Administrator | Agent | Client)
  - IsActive (bool)
  - CreatedAt (DateTime)

ClientDetail  [only if Role == Client]
  - Id (Guid)
  - UserId (Guid, FK)
  - CPF (string, unique)
  - BirthDate (DateOnly)
  - Phone (string)
  - Notes (string?)

ServiceType  [lookup table]
  - Id (int)
  - Description (string)
  - Examples: Consulting, Technical Support, Commercial Service, Interview

AgentAvailability
  - Id (Guid)
  - AgentId (Guid, FK → User)
  - WeekDay (enum: Monday ... Sunday)
  - StartTime (TimeOnly)
  - EndTime (TimeOnly)
  - IsActive (bool)
  - Rule: EndTime > StartTime
  - Rule: no overlapping intervals for the same agent + day

Appointment
  - Id (Guid)
  - Title (string)
  - Description (string?)
  - ServiceTypeId (int, FK)
  - ClientId (Guid, FK → User)
  - AgentId (Guid, FK → User)
  - Date (DateOnly)
  - Time (TimeOnly)
  - Status (enum — see below)
  - RejectionReason (string?)
  - ServiceSummary (string?)
  - CreatedAt (DateTime)
  - ConfirmedAt (DateTime?)
  - CanceledAt (DateTime?)
  - Notes (string?)
```

### Appointment Status Enum
```
PendingConfirmation
Confirmed
Rejected
Canceled
Completed
```

## Critical Business Rules

### Roles and Permissions

| Action | Administrator | Agent | Client |
|------|:---:|:---:|:---:|
| View all users | ✅ | ❌ | ❌ |
| Create user | ✅ | ❌ | ❌ |
| Edit any user | ✅ | ❌ | ❌ |
| Edit own user | ✅ | ✅ | ✅ |
| Delete user | ✅ | ❌ | ❌ |
| Create appointment | ❌ | ❌ | ✅ |
| View all appointments | ✅ | ❌ | ❌ |
| View own appointments | ✅ | ✅ (assigned) | ✅ |
| Confirm/Reject appointment | ❌ | ✅ | ❌ |
| Cancel appointment | ✅ (any) | ❌ | ✅ (with restrictions) |
| Mark as Completed | ❌ | ✅ | ❌ |
| Reassign agent | ✅ | ❌ | ❌ |
| Register availability | ✅ | ❌ | ❌ |
| View reports | ✅ | ✅ (restricted) | ❌ |

### Appointment Status Transition Rules

```
PendingConfirmation
  → Confirmed       (action: Agent confirms)
  → Rejected        (action: Agent rejects — RejectionReason is required)
  → Canceled        (action: Client or Administrator cancels)

Confirmed
  → Canceled        (action: Client [only if it has not occurred yet] or Administrator)
  → Completed       (action: Agent [only if appointment date/time has been reached])
```

### Client Cancellation
- Only when status is `PendingConfirmation` or `Confirmed`
- Only if the appointment date/time has not occurred yet

### Appointment Creation
- Date cannot be before the current date
- Time must be within an agent availability window
- There cannot be a conflict with another `Confirmed` or `PendingConfirmation` appointment for the same agent at the same time

### Availability
- EndTime > StartTime (required)
- Intervals cannot overlap for the same agent and weekday
- When querying available slots: subtract occupied slots from active appointments

## Non-Functional Requirements

- **NFR1** — Backend in C#/.NET 8+, PostgreSQL or SQL Server database, frontend with React + TypeScript
- **NFR2** — Frontend and backend in separate Docker containers
- **NFR3** — Minimum 70% unit test coverage in business rule classes, with no failing tests
- **NFR4** — Every route protected by JWT; role-based access control
- **NFR5** — Semantic HTTP responses: 200, 201, 400, 401, 403, 404, 500
- **NFR6** — Frontend shows user-friendly error messages for all API errors
- **NFR7** — Database structure via EF Core Migrations (no manual SQL)
- **NFR8** — Fields marked with (*) are required — validate on frontend and backend
- **NFR9** — Complete Swagger with request/response examples on all endpoints
- **NFR10** — Technical documentation with architecture diagrams, design decisions, and setup guide
- **NFR11** — Microservices are optional (not prioritized in the current timeline)

## Functional Modules

### FR1 — Users
- 1.1 Listing (filtered by role)
- 1.2 Creation (Admin only; extra fields for Client)
- 1.3 Editing (without changing email and password)

### FR2 — Appointments
- 2.1 Creation (Client only)
- 2.2 Listing with filters (Client, Agent, Type, Status, Period)
- 2.3 Details and role-based actions
- 2.4 Cancellation
- 2.5 Completion (mark as Completed)

### FR3 — Schedule Availability
- 3.1 Register time windows by weekday (Admin only)
- 3.2 Query available times when selecting agent + date

### FR4 — Reports
- Filters: Client(s), Agent(s), Period, Service Type, Status
- Report types: by agent, by client, by status, completed vs canceled rate, by type
- CSV and XLSX export
- Sortable table by any column
- Access: Administrator (full) and Agent (restricted to own data)

## Clean Architecture — Layers and Responsibilities

### Domain Layer
- Contains enterprise/business rules
- Entities, Value Objects, Domain Events, Interfaces, business rules
- **No dependencies on other layers**
- Structure: `AgendeX.Domain/Entities/`, `AgendeX.Domain/Interfaces/`

### Application Layer
- Contains application logic (use cases)
- Features with Commands/Queries, DTOs, service interfaces, MediatR handlers, validators
- **Depends only on Domain**
- Structure: `AgendeX.Application/Features/`, `AgendeX.Application/Common/`

### Infrastructure Layer
- Implements interfaces defined in Domain/Application
- Persistence (DbContext, configurations, migrations, repositories), services, and identity/JWT
- **Depends on Domain and Application**
- Structure: `AgendeX.Infrastructure/Persistence/`, `AgendeX.Infrastructure/Services/`, `AgendeX.Infrastructure/Identity/`

### WebAPI Layer
- Controllers, middlewares, Swagger setup, Program.cs
- **Depends only on Application** (must never reference Domain or Infrastructure directly)
- Structure: `AgendeX.WebAPI/Controllers/`, `AgendeX.WebAPI/Middlewares/`

Note: in the current implementation, startup composition in `Program.cs` wires Infrastructure through DI.

### Reference Structure

```
AgendeX.Domain/
├── Entities/
│   └── User.cs, ClientDetail.cs, RefreshToken.cs
├── Enums/
│   └── UserRole.cs
└── Interfaces/
  └── IUserRepository.cs, IRefreshTokenRepository.cs

AgendeX.Application/
├── Common/
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   └── Interfaces/
│       └── IPasswordHasher.cs, ITokenService.cs
├── Features/
│   └── Auth/
│       ├── Common/
│       │   └── AuthResponseDto.cs
│       └── Commands/
│           ├── Login/
│           │   └── LoginCommand.cs               # Command + Handler + Validator
│           ├── RefreshToken/
│           │   └── RefreshTokenCommand.cs        # Command + Handler + Validator
│           └── Logout/
│               └── LogoutCommand.cs              # Command + Handler + Validator
└── DependencyInjection.cs

AgendeX.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   └── UserConfiguration.cs, ClientDetailConfiguration.cs, RefreshTokenConfiguration.cs
│   ├── Migrations/
│   │   └── 20260417235513_InitialCreate.cs, ApplicationDbContextModelSnapshot.cs
│   └── Repositories/
│       └── UserRepository.cs, RefreshTokenRepository.cs, ...
├── Services/
│   └── TokenService.cs, PasswordHasher.cs
├── Identity/
│   └── JwtOptions.cs, RsaKeyProvider.cs
└── DependencyInjection.cs

AgendeX.WebAPI/
├── Controllers/
│   └── AuthController.cs
├── Models/
│   └── Auth/
│       └── LoginRequest.cs, RefreshRequest.cs, LogoutRequest.cs
├── Middlewares/
│   └── SecurityHeadersMiddleware.cs
└── Program.cs

AgendeX.Tests/
├── Application/
│   └── Auth/
│       └── AuthFlowTests.cs, LoginCommandHandlerTests.cs, RefreshTokenCommandHandlerTests.cs, LogoutCommandHandlerTests.cs, AuthValidatorsTests.cs
└── Infrastructure/
  └── Auth/
    └── TokenServiceTests.cs, PasswordHasherTests.cs
```

### Dependency Rule
```
WebAPI → Application → Domain
Infrastructure → Domain + Application
```
Never invert dependency direction. Domain must never import from other layers.

## Coding Standards

### Backend
- One MediatR handler per use case (never put business logic in controllers)
- Thin controllers: receive request → dispatch command/query → return result
- Validation with FluentValidation in each use case (prefer same file as the command for fast delivery)
- Prefer one file per use case in Application, e.g. `*Command.cs` containing Command + Handler + Validator
- Organize Application by `Features/<Module>/Commands|Queries/<UseCase>/` and shared contracts/utilities in `Common/`
- Repositories via Domain interfaces, implemented in Infrastructure
- Do not use `var` when the type is not obvious
- Methods with a maximum of 20 lines — extract when needed
- Portuguese names for domain entities, English for technical infrastructure

### Tests
- One test file per handler
- Naming: `MethodName_Scenario_ExpectedResult`
- Always mock repositories with Moq
- Use FluentAssertions for readable assertions

### Frontend
- Functional components with strict TypeScript
- Custom hooks for business logic (never directly in components)
- React Query for cache and loading/error states
- Zod for form validation
- Never use `any` — always type everything

## Evaluation Criteria (weights)

1. Technical knowledge (weight 2)
2. Planning and organization
3. Communication and interaction
4. Collaborative work
5. Analysis and synthesis

## Deadline

**04/19/2026 at 23:59** — mandatory submission via repository + Pandapé

## Recommended Implementation Order

1. Base structure + Docker Compose + initial migrations
2. JWT authentication (login, token generation, middleware)
3. Users module (full CRUD)
4. Availability module (prerequisite for appointments)
5. Appointments module (creation, listing, actions, cancellation, completion)
6. Reports module (queries + export)
7. Unit tests for handlers
8. Technical documentation (README, Mermaid diagrams)

## STAGE 2 — JWT Authentication (Secure)

**Estimate:** ~3h

### Tokens

- Access token with 15-minute expiration
- Refresh token with 7-day expiration, stored in the database as a hash
- **RS256** algorithm (asymmetric RSA key pair), do not use HS256
- Access token claims:
  - `sub` (userId)
  - `name`
  - `email`
  - `role`
  - `jti` (unique GUID for tracing)

### RefreshToken Entity (Infrastructure)

```csharp
RefreshToken
  - Id (Guid)
  - UserId (Guid, FK)
  - TokenHash (string)   // store SHA-256 hash, never store plain-text token
  - ExpiresAt (DateTime)
  - IsRevoked (bool)
  - CreatedAt (DateTime)
```

### Authentication Endpoints

| Method | Route | Description |
|------|------|------|
| POST | /api/auth/login | Validates credentials and returns `{ accessToken, refreshToken, expiresAt }` |
| POST | /api/auth/refresh | Receives refresh token, issues a new pair, revokes the previous one (rotation) |
| POST | /api/auth/logout | Revokes refresh token in the database |

### Password Protection

- BCrypt with work factor **12** (do not use 10)

### Rate Limiting

- Package: `AspNetCoreRateLimit`
- Rule: maximum 5 login attempts per IP in 1 minute
- Limit exceeded: return `429 Too Many Requests`
- Configuration via `appsettings.json` (easy to demonstrate in Swagger)

### Security Headers

Custom middleware adding:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: no-referrer`
- `X-XSS-Protection: 1; mode=block`

### Swagger

- Bearer Token support configured
- Request/response examples on all auth endpoints
- Status code documentation: 200, 400, 401, 429
