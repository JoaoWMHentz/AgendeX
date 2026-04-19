# AgendeX — Project Context for Claude Code

## Overview

Web system for managing service appointments between clients and agents/specialists. Developed as a practical assessment for the SENAI/FIESC hiring process (01064/2026).

## Stack

### Backend
   ├── .claude/
   │   └── settings.local.json
   ├── .github/
   │   └── copilot-instructions.md
- FluentValidation 12.1
- JWT RS256 (Microsoft.IdentityModel.Tokens 8.14)
- Swagger via Swashbuckle.AspNetCore 10
- AspNetCoreRateLimit 5.0
- BCrypt.Net-Next 4.0 (work factor 12)
- React Router DOM
- React Query (TanStack Query)
- React Hook Form + Zod
├── .claude/
│   └── settings.local.json
├── .github/
│   └── copilot-instructions.md
- Axios
- Ant Design (UI library)
- Zustand

### Infrastructure
- Docker + Docker Compose
- PostgreSQL 16

## Folder Structure

```
prova-dotnet-react-senior-01064-2026-096.688.359-48/
├── .claude/
├── .github/
├── backend/
│   ├── AgendeX.slnx
│   ├── AgendeX.Domain/              # Entities, enums, interfaces
│   ├── AgendeX.Application/         # Features (Commands/Queries) + Common (Behaviors/Interfaces)
│   ├── AgendeX.Infrastructure/      # Persistence (EF/repositories/migrations), Services, Identity
│   ├── AgendeX.WebAPI/              # Controllers, Swagger, middlewares, serialization, Program.cs
│   ├── AgendeX.Tests/               # Unit tests (xUnit + Moq)
│   └── scripts/                     # Utility scripts (seed, etc.)
├── frontend/                        # React app (Vite + TypeScript + Ant Design)
│   └── src/
│       ├── app/                     # Providers, router, theme, app bootstrap
│       ├── features/                # Feature-based modules (auth, users, appointments, availability, service-types)
│       ├── services/                # API client and service adapters
│       └── shared/                  # Reusable UI, constants, utils, query keys
├── docker-compose.yml               # Local orchestration
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
  │   │   │   │   ├── 20260417235513_InitialCreate.Designer.cs
  │   │   │   │   ├── 20260418172619_AddAvailabilityAndAppointments.Designer.cs
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
│   │   │   │   ├── 20260417235513_InitialCreate.Designer.cs

│   │   │   │   ├── 20260418172619_AddAvailabilityAndAppointments.Designer.cs
│   ├── .env
│   ├── .env.example
│   ├── package-lock.json
│   ├── package.json
│   └── src/
│       ├── app/
│       │   ├── AppLayout.tsx
│       │   ├── ProtectedRoute.tsx
│       │   ├── providers.tsx
│       │   ├── queryClient.ts
│       │   ├── router.tsx
│       │   ├── theme.ts
│       │   └── themes/
│       ├── services/
│       │   ├── api.ts
│       │   ├── appointments.service.ts
│       │   ├── auth.service.ts
│       │   ├── availability.service.ts
│       │   ├── service-types.service.ts
│       │   ├── tokenStorage.ts
│       │   └── users.service.ts
│       ├── shared/
│       │   ├── components/
│       │   │   ├── DatePickerField.tsx
│       │   │   ├── FormModal.tsx
│       │   │   └── TimePickerField.tsx
│       │   ├── constants/
│       │   │   └── roles.ts
│       │   ├── queryKeys.ts
│       │   ├── types/
│       │   │   └── index.ts
│       │   └── utils/
│       │       ├── apiError.ts
│       │       └── masks.ts
│       └── features/
│           ├── auth/
│           │   ├── authStore.ts
│           │   ├── jwtUtils.ts
│           │   ├── LoginPage.tsx
│           │   └── types.ts
│           ├── users/
│           │   ├── components/
│           │   ├── hooks/
│           │   ├── models/
│           │   └── pages/
│           ├── appointments/
│           │   ├── components/
│           │   ├── hooks/
│           │   ├── pages/
│           │   ├── types.ts
│           │   └── useAppointments.ts
│           ├── availability/
│           │   ├── components/
│           │   ├── hooks/
│           │   ├── pages/
│           │   ├── types.ts
│           │   └── useAvailability.ts
│           └── service-types/
│               ├── ServiceTypesPage.tsx
│               ├── types.ts
│               └── useServiceTypes.ts

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
.
├── .claude/
│   └── settings.local.json
├── .github/
│   └── copilot-instructions.md
├── backend/
│   ├── AgendeX.slnx
│   ├── AgendeX.Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs
│   │   │   └── Interfaces/
│   │   │       ├── ICurrentUserService.cs
│   │   │       ├── IPasswordHasher.cs
│   │   │       └── ITokenService.cs
│   │   ├── Features/
│   │   │   ├── Appointments/
│   │   │   │   ├── AppointmentDto.cs
│   │   │   │   ├── CancelAppointment/
│   │   │   │   ├── CompleteAppointment/
│   │   │   │   ├── ConfirmAppointment/
│   │   │   │   ├── CreateAppointment/
│   │   │   │   ├── GetAppointmentById/
│   │   │   │   ├── GetAppointments/
│   │   │   │   ├── ReassignAppointment/
│   │   │   │   └── RejectAppointment/
│   │   │   ├── Auth/
│   │   │   │   ├── AuthDto.cs
│   │   │   │   ├── Login/
│   │   │   │   ├── Logout/
│   │   │   │   └── RefreshToken/
│   │   │   ├── Availability/
│   │   │   │   ├── AvailabilityDto.cs
│   │   │   │   ├── CreateAvailability/
│   │   │   │   ├── DeleteAvailability/
│   │   │   │   ├── GetAvailableSlots/
│   │   │   │   ├── GetAvailabilitiesByAgent/
│   │   │   │   └── UpdateAvailability/
│   │   │   ├── ServiceTypes/
│   │   │   │   ├── ServiceTypeDto.cs
│   │   │   │   ├── GetServiceTypeById/
│   │   │   │   └── GetServiceTypes/
│   │   │   └── Users/
│   │   │       ├── UserDto.cs
│   │   │       ├── CreateUser/
│   │   │       ├── DeleteUser/
│   │   │       ├── GetUserById/
│   │   │       ├── GetUsers/
│   │   │       ├── SetClientDetail/
│   │   │       └── UpdateUser/
│   │   └── DependencyInjection.cs
│   ├── AgendeX.Domain/
│   │   ├── Entities/
│   │   │   ├── AgentAvailability.cs
│   │   │   ├── Appointment.cs
│   │   │   ├── ClientDetail.cs
│   │   │   ├── RefreshToken.cs
│   │   │   ├── ServiceType.cs
│   │   │   └── User.cs
│   │   ├── Enums/
│   │   │   ├── AppointmentStatus.cs
│   │   │   ├── UserRole.cs
│   │   │   └── WeekDay.cs
│   │   └── Interfaces/
│   │       ├── IAgentAvailabilityRepository.cs
│   │       ├── IAppointmentRepository.cs
│   │       ├── IClientDetailRepository.cs
│   │       ├── IRefreshTokenRepository.cs
│   │       ├── IServiceTypeRepository.cs
│   │       └── IUserRepository.cs
│   ├── AgendeX.Infrastructure/
│   │   ├── DependencyInjection.cs
│   │   ├── Identity/
│   │   │   ├── JwtOptions.cs
│   │   │   └── RsaKeyProvider.cs
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── AgentAvailabilityConfiguration.cs
│   │   │   │   ├── AppointmentConfiguration.cs
│   │   │   │   ├── ClientDetailConfiguration.cs
│   │   │   │   ├── RefreshTokenConfiguration.cs
│   │   │   │   ├── ServiceTypeConfiguration.cs
│   │   │   │   └── UserConfiguration.cs
│   │   │   ├── Migrations/
│   │   │   │   ├── 20260417235513_InitialCreate.cs
│   │   │   │   ├── 20260417235513_InitialCreate.Designer.cs
│   │   │   │   ├── 20260418172619_AddAvailabilityAndAppointments.cs
│   │   │   │   ├── 20260418172619_AddAvailabilityAndAppointments.Designer.cs
│   │   │   │   └── ApplicationDbContextModelSnapshot.cs
│   │   │   └── Repositories/
│   │   │       ├── AgentAvailabilityRepository.cs
│   │   │       ├── AppointmentRepository.cs
│   │   │       ├── ClientDetailRepository.cs
│   │   │       ├── RefreshTokenRepository.cs
│   │   │       ├── ServiceTypeRepository.cs
│   │   │       └── UserRepository.cs
│   │   └── Services/
│   │       ├── PasswordHasher.cs
│   │       └── TokenService.cs
│   ├── AgendeX.Tests/
│   │   ├── Application/
│   │   │   ├── Appointments/
│   │   │   │   ├── AppointmentLifecycleHandlersTests.cs
│   │   │   │   ├── AppointmentQueriesTests.cs
│   │   │   │   ├── AppointmentValidatorsTests.cs
│   │   │   │   ├── CreateAppointmentCommandHandlerTests.cs
│   │   │   │   └── ReassignAppointmentCommandHandlerTests.cs
│   │   │   ├── Availability/
│   │   │   │   ├── AvailabilityQueriesTests.cs
│   │   │   │   ├── AvailabilityValidatorsTests.cs
│   │   │   │   ├── CreateAvailabilityCommandHandlerTests.cs
│   │   │   │   └── UpdateAndDeleteAvailabilityHandlersTests.cs
│   │   │   └── Common/
│   │   │       └── EntityTestFactory.cs
│   │   └── Infrastructure/
│   │       ├── Auth/
│   │       │   ├── PasswordHasherTests.cs
│   │       │   ├── RsaKeyProviderTests.cs
│   │       │   └── TokenServiceTests.cs
│   │       └── Persistence/
│   │           ├── EntityConfigurationTests.cs
│   │           ├── RefreshTokenRepositoryTests.cs
│   │           └── UserRepositoryTests.cs
│   ├── AgendeX.WebAPI/
│   │   ├── Controllers/
│   │   │   ├── AppointmentsController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── AvailabilityController.cs
│   │   │   ├── ServiceTypesController.cs
│   │   │   └── UsersController.cs
│   │   ├── Middlewares/
│   │   │   ├── AuthorizeOperationFilter.cs
│   │   │   ├── GlobalExceptionHandler.cs
│   │   │   ├── SecurityHeadersMiddleware.cs
│   │   │   └── SwaggerExamplesOperationFilter.cs
│   │   ├── Properties/
│   │   ├── Serialization/
│   │   │   └── TimeOnlyMinutesJsonConverter.cs
│   │   ├── Services/
│   │   │   └── CurrentUserService.cs
│   │   ├── appsettings.json
│   │   ├── AgendeX.WebAPI.csproj
│   │   ├── AgendeX.WebAPI.csproj.user
│   │   ├── Dockerfile
│   │   └── Program.cs
│   └── scripts/
│       └── seed-auth-user.sql
├── frontend/
│   ├── .env
│   ├── .env.example
│   ├── package-lock.json
│   ├── package.json
│   └── src/
│       ├── app/
│       │   ├── AppLayout.tsx
│       │   ├── ProtectedRoute.tsx
│       │   ├── providers.tsx
│       │   ├── queryClient.ts
│       │   ├── router.tsx
│       │   ├── theme.ts
│       │   └── themes/
│       ├── services/
│       │   ├── api.ts
│       │   ├── appointments.service.ts
│       │   ├── auth.service.ts
│       │   ├── availability.service.ts
│       │   ├── service-types.service.ts
│       │   ├── tokenStorage.ts
│       │   └── users.service.ts
│       ├── shared/
│       │   ├── components/
│       │   │   ├── DatePickerField.tsx
│       │   │   ├── FormModal.tsx
│       │   │   └── TimePickerField.tsx
│       │   ├── constants/
│       │   │   └── roles.ts
│       │   ├── queryKeys.ts
│       │   ├── types/
│       │   │   └── index.ts
│       │   └── utils/
│       │       ├── apiError.ts
│       │       └── masks.ts
│       └── features/
│           ├── auth/
│           │   ├── authStore.ts
│           │   ├── jwtUtils.ts
│           │   ├── LoginPage.tsx
│           │   └── types.ts
│           ├── users/
│           │   ├── components/
│           │   ├── hooks/
│           │   ├── models/
│           │   └── pages/
│           ├── appointments/
│           │   ├── components/
│           │   ├── hooks/
│           │   ├── pages/
│           │   ├── types.ts
│           │   └── useAppointments.ts
│           ├── availability/
│           │   ├── components/
│           │   ├── hooks/
│           │   ├── pages/
│           │   ├── types.ts
│           │   └── useAvailability.ts
│           └── service-types/
│               ├── ServiceTypesPage.tsx
│               ├── types.ts
│               └── useServiceTypes.ts
└── README.md
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
- Ant Design as the primary UI library in this MVP (do not mix with shadcn/ui in this phase)
- Custom hooks for business logic (never directly in components)
- React Query for cache and loading/error states
- Zod for form validation
- Never use `any` — always type everything
- Organize by feature using the pattern: `pages` (composition), `components` (presentational), `hooks` (state and business flow), `types` (feature contracts)
- Keep pages thin: pages should orchestrate composition and delegate behavior to hooks/components

## Frontend Plan (Ant Design)

### Goal
- Deliver a simple, efficient, and well-organized frontend using Ant Design, aligned with the current API and business rules.

### Architecture Decisions
- UI library: Ant Design
- Data/state: React Query for server state + Zustand for lightweight session/global state
- Forms: React Hook Form + Zod
- HTTP: Axios with interceptors for JWT access token and refresh token flow
- Routing: React Router DOM with role-based route guards

### Folder Organization (frontend/src)
- app (providers, router, bootstrap, theme)
- shared (common ui wrappers, utils, constants, query keys)
- services (api client, auth/token management, endpoint services)
- features (feature-first modules)

### Frontend Feature Pattern
- Each feature should follow this baseline structure:
  - `pages/`: route-level components and composition only
  - `components/`: reusable local UI components for the feature
  - `hooks/`: React Query hooks and page controllers/use-cases
  - `types/`: feature-specific types, enums, and DTO contracts

Reference (current users module):

```
frontend/src/features/users/
├── models/
│   └── types.ts
├── pages/
│   ├── UsersPage.tsx
│   └── ProfilePage.tsx
├── components/
│   ├── UsersList.tsx
│   ├── CreateUserModal.tsx
│   └── EditUserModal.tsx
├── hooks/
│   ├── useUsers.ts
│   └── useUsersPageController.ts
```

### Organization for Other Frontend Modules

```
frontend/src/features/
├── auth/
│   ├── authStore.ts
│   ├── jwtUtils.ts
│   ├── LoginPage.tsx
│   └── types.ts
├── appointments/
│   ├── pages/
│   ├── components/
│   ├── hooks/
│   ├── types.ts
│   └── useAppointments.ts
├── availability/
│   ├── pages/
│   ├── components/
│   ├── hooks/
│   ├── types.ts
│   └── useAvailability.ts
├── service-types/
│   ├── ServiceTypesPage.tsx
│   ├── types.ts
│   └── useServiceTypes.ts
└── users/
    ├── pages/
    ├── components/
    ├── hooks/
    └── models/
```

Notes:
- `hooks/` may include both API hooks (React Query) and page controller hooks.
- Feature logic must stay inside its own feature folder; only truly shared code goes to `shared/`.
- Keep `services/` as API adapters; avoid putting UI state or feature orchestration there.

### Delivery Phases
1. Foundation
  - Bootstrap Vite + React + TypeScript project
  - Install core libs (Ant Design, React Query, React Hook Form, Zod, Axios, Zustand, Router)
  - Configure providers, app layout shell, and environment variables
2. Authentication and Access
  - Login, logout, token refresh flow
  - Protected routes and role guards (Administrator, Agent, Client)
3. Core Modules (MVP)
  - Appointments: list with filters, detail, and role-based actions
  - Availability: CRUD for admin and slots query
  - Users: list/create/update/set client detail
  - Service Types: list for filters/select inputs
4. UX and Error Handling
  - Central API error mapping to user-friendly messages
  - Consistent loading, empty states, and success/error feedback
5. Infrastructure
  - Frontend Dockerfile
  - docker-compose update to orchestrate backend + frontend + database

### Scope Notes
- Reports module (FR4) starts with navigation and structure; advanced exports (CSV/XLSX) can be delivered in a subsequent phase.
- Prioritize completion of FR1, FR2, and FR3 with stable auth and permissions.

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

## Frontend — Implementation Status

### Done
- Foundation: Vite + React + TS + Ant Design + React Query + RHF + Zod + Axios + Zustand + Router
- Auth: login, logout, refresh token, JWT decode, Zustand session store, Axios interceptors
- Layout: AppLayout (sidebar + header), ProtectedRoute (auth + role), theme (dark/light)
- Users (Admin): list, create, edit, set client detail, delete — fully functional
- Service Types: list — fully functional
- Availability (Admin): list by agent, create, edit, delete windows — fully functional
- Appointments (Admin/Agent): list with filters, confirm, reject, complete, reassign, cancel
- Agent flow: dedicated page `/agent/my-appointments` with role-based menu and scoped actions
- Client flow: dedicated pages with role-based menu
  - `/client/new-appointment` — form (title, description, service type, date) + slots table per agent + confirm modal
  - `/client/my-appointments` — own appointments with cancel action
- Role-based routing:
  - Admin → `/appointments`, `/users`, `/service-types`
  - Admin+Agent → `/availability`
  - Agent → `/agent/my-appointments`
  - Client → `/client/*`
- Shared components: FormModal, DatePickerField, TimePickerField

### Pending
- [ ] Dockerfile do frontend e atualização do docker-compose para orquestrar a aplicação
- [ ] Reports module (FR4) — queries + CSV/XLSX export
- [ ] README e diagramas de arquitetura (Mermaid)
