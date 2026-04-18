# AgendeX — Project Context for Claude Code

## Overview

Web system for managing service appointments between clients and agents/specialists. Developed as a practical assessment for the SENAI/FIESC hiring process (01064/2026).

## Stack

### Backend
- .NET 10 — ASP.NET Core Web API
- Clean Architecture (Domain / Application / Infrastructure / WebAPI)
- CQRS with MediatR 12.5
- Entity Framework Core 9 + Npgsql (PostgreSQL 16)
- FluentValidation 12.1
- JWT RS256 (Microsoft.IdentityModel.Tokens 8.14)
- Swagger via Swashbuckle.AspNetCore 10
- AspNetCoreRateLimit 5.0
- BCrypt.Net-Next 4.0 (work factor 12)
- xUnit + Moq + FluentAssertions + EF InMemory (tests)

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
├── frontend/                        # React app (not yet started)
├── docker-compose.yml               # PostgreSQL 16 only (app containers pending)
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

ServiceType  [lookup table — seeded]
  - Id (int)
  - Description (string)
  - Seeds: Consulting (1), Technical Support (2), Commercial Service (3), Interview (4)

AgentAvailability
  - Id (Guid)
  - AgentId (Guid, FK → User)
  - WeekDay (enum: Sunday=0 ... Saturday=6)
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
| Set client detail | ✅ | ❌ | ❌ |
| Create appointment | ❌ | ❌ | ✅ |
| View all appointments | ✅ | ❌ | ❌ |
| View own appointments | ✅ | ✅ (assigned) | ✅ |
| Confirm/Reject appointment | ❌ | ✅ | ❌ |
| Cancel appointment | ✅ (any) | ❌ | ✅ (with restrictions) |
| Mark as Completed | ❌ | ✅ | ❌ |
| Reassign agent | ✅ | ❌ | ❌ |
| Register availability | ✅ | ❌ | ❌ |
| View availability | ✅ | ✅ | ✅ |
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
- 1.1 Listing (filtered by role) — Admin only
- 1.2 Creation (Admin only) — `POST /api/users` with `{name, email, password, role}`
- 1.3 Set client detail — `PUT /api/users/{id}/client-detail` — validates user is Client role
- 1.4 Editing name — `PUT /api/users/{id}` with `{name}`

### FR2 — Appointments
- 2.1 Creation (Client only)
- 2.2 Listing with filters (ClientId, AgentId, ServiceTypeId, Status, From, To)
- 2.3 Details and role-based actions
- 2.4 Cancellation
- 2.5 Completion (mark as Completed)
- 2.6 Reassign agent (Admin only)

### FR3 — Schedule Availability
- 3.1 Register time windows by weekday (Admin only)
- 3.2 Query availabilities by agent
- 3.3 Query available slots for a specific agent + date (deducts confirmed/pending appointments)

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

### Application Layer
- Contains application logic (use cases)
- Features with Commands/Queries, DTOs, MediatR handlers, FluentValidation validators
- **Depends only on Domain**

### Infrastructure Layer
- Implements interfaces defined in Domain/Application
- Persistence (DbContext, configurations, migrations, repositories), services, and identity/JWT
- **Depends on Domain and Application**

### WebAPI Layer
- Controllers, middlewares, Swagger setup, Program.cs
- **Depends only on Application** (startup composition in Program.cs wires Infrastructure through DI)

### Dependency Rule
```
WebAPI → Application → Domain
Infrastructure → Domain + Application
```

## Current File Structure

```
AgendeX.Domain/
├── Entities/
│   └── User.cs, ClientDetail.cs, RefreshToken.cs
│   └── ServiceType.cs, AgentAvailability.cs, Appointment.cs
├── Enums/
│   └── UserRole.cs           # Administrator | Agent | Client
│   └── WeekDay.cs            # Sunday=0 ... Saturday=6
│   └── AppointmentStatus.cs  # PendingConfirmation | Confirmed | Rejected | Canceled | Completed
└── Interfaces/
    └── IUserRepository.cs, IClientDetailRepository.cs, IRefreshTokenRepository.cs
    └── IServiceTypeRepository.cs, IAgentAvailabilityRepository.cs, IAppointmentRepository.cs

AgendeX.Application/
├── Common/
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   └── Interfaces/
│       └── IPasswordHasher.cs, ITokenService.cs
├── Features/
│   ├── Auth/
│   │   ├── AuthDto.cs          # AuthResponseDto
│   │   └── AuthCommands.cs     # Login + Refresh + Logout (Command + Handler + Validator each)
│   ├── Users/
│   │   ├── UserDto.cs          # UserDto + ClientDetailDto + UserMapper
│   │   ├── UserQueries.cs      # GetUsers + GetUserById
│   │   └── UserCommands.cs     # CreateUser + UpdateUser + DeleteUser + SetClientDetail
│   ├── ServiceTypes/
│   │   ├── ServiceTypeDto.cs
│   │   └── ServiceTypeQueries.cs   # GetServiceTypes + GetServiceTypeById
│   ├── Availability/
│   │   ├── AvailabilityDto.cs       # AvailabilityDto + AvailableSlotDto
│   │   ├── AvailabilityQueries.cs   # GetAvailabilitiesByAgent + GetAvailableSlots
│   │   └── AvailabilityCommands.cs  # CreateAvailability + UpdateAvailability + DeleteAvailability
│   └── Appointments/
│       ├── AppointmentDto.cs        # AppointmentDto + AppointmentMapper
│       ├── AppointmentQueries.cs    # GetAppointments + GetAppointmentById
│       └── AppointmentCommands.cs   # Create + Confirm + Reject + Cancel + Complete + Reassign
└── DependencyInjection.cs

AgendeX.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   └── UserConfiguration.cs, ClientDetailConfiguration.cs, RefreshTokenConfiguration.cs
│   │   └── ServiceTypeConfiguration.cs, AgentAvailabilityConfiguration.cs, AppointmentConfiguration.cs
│   ├── Migrations/
│   │   └── 20260417235513_InitialCreate.cs
│   │   └── 20260418172619_AddAvailabilityAndAppointments.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   └── Repositories/
│       └── UserRepository.cs, ClientDetailRepository.cs, RefreshTokenRepository.cs
│       └── ServiceTypeRepository.cs, AgentAvailabilityRepository.cs, AppointmentRepository.cs
├── Services/
│   └── TokenService.cs, PasswordHasher.cs
├── Identity/
│   └── JwtOptions.cs, RsaKeyProvider.cs   # RSA key pair gerado em memória com KeyId derivado via SHA-256
└── DependencyInjection.cs

AgendeX.WebAPI/
├── Controllers/
│   └── AuthController.cs           # POST /api/auth/login, /refresh, /logout
│   └── UsersController.cs          # GET/POST/PUT/DELETE /api/users + PUT /api/users/{id}/client-detail
│   └── ServiceTypesController.cs   # GET /api/servicetypes, GET /api/servicetypes/{id}
│   └── AvailabilityController.cs   # GET /api/availability/agent/{id}, GET /api/availability/slots, POST/PUT/DELETE
│   └── AppointmentsController.cs   # GET/POST /api/appointments + PUT /confirm /reject /cancel /complete /reassign
├── Middlewares/
│   └── SecurityHeadersMiddleware.cs
│   └── GlobalExceptionHandler.cs   # ValidationException→400, KeyNotFound→404, Unauthorized→401, other→500
│   └── AuthorizeOperationFilter.cs # Swagger: remove lock em /api/auth/*, adiciona JWT nos demais
└── Program.cs

AgendeX.Tests/
├── Application/
│   └── Auth/
│       └── AuthFlowTests.cs, LoginCommandHandlerTests.cs, RefreshTokenCommandHandlerTests.cs
│       └── LogoutCommandHandlerTests.cs, AuthValidatorsTests.cs
└── Infrastructure/
    ├── Auth/
    │   └── TokenServiceTests.cs, PasswordHasherTests.cs, RsaKeyProviderTests.cs
    └── Persistence/
        └── UserRepositoryTests.cs, RefreshTokenRepositoryTests.cs, EntityConfigurationTests.cs
```

## API Endpoints (implemented)

### Auth
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | /api/auth/login | ❌ | Returns `{ accessToken, refreshToken, expiresAt }` |
| POST | /api/auth/refresh | ❌ | Rotates refresh token |
| POST | /api/auth/logout | ❌ | Revokes refresh token |

### Users
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | /api/users | Admin | List users (filter by role) |
| GET | /api/users/{id} | Any | Get user by ID |
| POST | /api/users | Admin | Create user `{name, email, password, role}` |
| PUT | /api/users/{id} | Any | Update name |
| PUT | /api/users/{id}/client-detail | Admin | Set/update client detail (validates role == Client) |
| DELETE | /api/users/{id} | Admin | Soft-delete (deactivates) |

### Service Types
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | /api/servicetypes | Any | List all service types |
| GET | /api/servicetypes/{id} | Any | Get by ID |

### Availability
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | /api/availability/agent/{agentId} | Any | List availability windows for an agent |
| GET | /api/availability/slots?agentId=&date= | Any | Available time slots (deducts occupied) |
| POST | /api/availability | Admin | Create availability window |
| PUT | /api/availability/{id} | Admin | Update start/end time |
| DELETE | /api/availability/{id} | Admin | Deactivate window |

### Appointments
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | /api/appointments | Any | List with filters; scope enforced by role |
| GET | /api/appointments/{id} | Any | Get details |
| POST | /api/appointments | Client | Create appointment |
| PUT | /api/appointments/{id}/confirm | Agent | Confirm |
| PUT | /api/appointments/{id}/reject | Agent | Reject (requires reason) |
| PUT | /api/appointments/{id}/cancel | Client/Admin | Cancel |
| PUT | /api/appointments/{id}/complete | Agent | Mark completed |
| PUT | /api/appointments/{id}/reassign | Admin | Reassign to another agent |

## Coding Standards

### Backend
- One MediatR handler per use case (never put business logic in controllers)
- **Thin controllers** — receive request → dispatch command/query → return result
- **Command-as-body pattern**: when all command fields come from the request body, pass the command directly as `[FromBody]`; when the command has fields that come from the route or JWT token (e.g. `Id`, `UserId`), use a minimal `*Body` record for the body-only fields and construct the command manually
  ```csharp
  // ✅ All fields from body — use command directly
  public async Task<IActionResult> Create([FromBody] CreateUserCommand command, ...)

  // ✅ Id from route, fields from body — use *Body record
  public sealed record UpdateUserBody(string Name);
  public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserBody body, ...)
      => Ok(await _sender.Send(new UpdateUserCommand(id, body.Name), ct));
  ```
- **Role strings**: always use the `Roles` static class (`Roles.Administrator`, `Roles.Agent`, `Roles.Client`) in `[Authorize]` attributes — never hardcode strings
- **Current user**: inject `ICurrentUserService` (Application layer interface) into controllers that need the authenticated user's ID or role — never parse JWT claims manually in controllers
- Validation with FluentValidation in each use case, same file as the command
- Organize Application by `Features/<Module>/`: `*Dto.cs`, `*Queries.cs`, `*Commands.cs`
- Repositories via Domain interfaces, implemented in Infrastructure
- Do not use `var` when the type is not obvious
- Methods with a maximum of 20 lines — extract when needed

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

## Authentication Details

- JWT RS256 — asymmetric RSA 2048-bit key pair generated in memory at startup (`RsaKeyProvider`)
- `KeyId` derived via SHA-256 of RSA modulus (no hardcode, unique per instance)
- Access token: 15 min — claims: `sub`, `name`, `email`, `role` (ClaimTypes.Role URI), `jti`
- Refresh token: 7 days, stored as SHA-256 hash in DB, rotation on refresh
- Rate limit: max 5 login attempts per IP per minute (AspNetCoreRateLimit)
- Fallback authorization policy: all routes require auth by default
- Security headers middleware: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `X-XSS-Protection`

## Evaluation Criteria (weights)

1. Technical knowledge (weight 2)
2. Planning and organization
3. Communication and interaction
4. Collaborative work
5. Analysis and synthesis

## Deadline

**04/19/2026 at 23:59** — mandatory submission via repository + Pandapé

## Pending

- [ ] Frontend React (pasta vazia)
- [ ] Dockerfiles de app (backend + frontend) + atualizar docker-compose
- [ ] Reports module (FR4) — queries + CSV/XLSX export
- [ ] Unit tests para novos handlers (Availability, Appointments)
- [ ] README + diagramas Mermaid
