# Identity Module

`Modules/Identity` owns authentication, authorization, identity administration, and permission assignment for the modular monolith.

## Current snapshot

### What exists now

- JWT login, refresh, logout, and change-password flows are implemented
- JWT bearer authentication is configured in the host
- Identity controllers are module-gated with `RequireModule("Identity")`
- permission enforcement uses ASP.NET Core auth plus the database-backed `DbPermissionGate`
- current user resolution is claims-based through `ICurrentUser`
- authorization entities exist: `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, `UserPermission`, `UserSession`
- EF Core persistence is used through `IdentityDbContext`
- Identity schema is applied automatically at host startup via `MigrateIdentityModuleAsync()`
- seed users exist: `admin`, `manager`, `cashier`
- seed roles exist: `ADMIN`, `MANAGER`, `CASHIER`

### Current request pipeline

1. `UseAuthentication()` validates JWT and populates `HttpContext.User`
2. `UseFeatureGate()` reads endpoint metadata
3. `[RequireModule]` returns `404` when the module is disabled
4. `[RequireFeature]` returns `404` when the feature is disabled
5. permission-protected endpoints require authenticated access first, then permission access
6. unauthenticated access to a protected endpoint returns `401`
7. authenticated access without permission returns `403`

### Current user resolution

`ICurrentUser` exposes:

- `UserId`
- `UserName`
- `IsAuthenticated`

`HeaderCurrentUserAccessor` resolves user data from claims first.

For `UserId`:
- `ClaimTypes.NameIdentifier`
- `sub`

For `UserName`:
- `preferred_username`
- `sub`
- `ClaimTypes.NameIdentifier`

In development only, it can fall back to headers:

- `X-Commerce-UserId`
- `X-Commerce-UserName`

### Current permission source

The active permission source is `DbPermissionGate`.

Resolution order:

1. user direct deny
2. user direct allow
3. role deny
4. role allow
5. default deny

## API coverage now

Implemented:

- `POST /api/identity/login`
- `POST /api/identity/refresh`
- `POST /api/identity/logout`
- `POST /api/identity/change-password`
- `GET /api/identity/users`
- `POST /api/identity/users`
- `PUT /api/identity/users/{id}`
- `DELETE /api/identity/users/{id}`
- `POST /api/identity/users/{id}/reset-password`
- `GET /api/identity/roles`
- `POST /api/identity/roles`
- `PUT /api/identity/roles/{id}`
- `DELETE /api/identity/roles/{id}`
- `GET /api/identity/permissions`
- `POST /api/identity/permissions`
- `PUT /api/identity/permissions/{id}`
- `DELETE /api/identity/permissions/{id}`
- `GET /api/identity/users/{userId}/roles`
- `PUT /api/identity/users/{userId}/roles`
- `GET /api/identity/users/{userId}/permissions`
- `PUT /api/identity/users/{userId}/permissions`
- `GET /api/identity/roles/{roleId}/permissions`
- `PUT /api/identity/roles/{roleId}/permissions`
- `GET /api/identity/me`
- `GET /api/identity/my-permissions`
- `GET /api/identity/authorization/overview`
- `GET /api/identity/health`

Still intentionally missing:

- access-token blacklist / immediate access-token revocation
- forgot-password email/self-service flow
- session/device management UI
- management UI or workflow docs for admins

## Database wiring

The system currently assumes `1 deployment = 1 database`.

`IdentityDbContext` is registered by the host using `ConnectionStrings:Default`. The shared default database wiring currently uses PostgreSQL via `UseNpgsql`. The module does not read connection strings directly and does not own provider selection.

## Migrations

Identity now owns committed EF Core migrations under `Infrastructure/Persistence/Migrations`.

Runtime behavior:

- the host calls `MigrateIdentityModuleAsync()` during startup
- the module applies pending Identity migrations through `Database.MigrateAsync()`
- a clean database should be able to bootstrap the Identity schema and seed data from committed migrations

To add a new Identity migration:

```bash
dotnet ef migrations add <MigrationName> \
  --context Commerce.Modules.Identity.Infrastructure.IdentityDbContext \
  --project Modules/Identity/Commerce.Modules.Identity/Commerce.Modules.Identity.csproj \
  --startup-project Host/WebApi/Commerce.Host.WebApi/Commerce.Host.WebApi.csproj \
  --output-dir Infrastructure/Persistence/Migrations
```

## Seed data and bootstrap rules

The module seeds system users, roles, permissions, and default assignments.

Seeded users:

- `admin`
- `manager`
- `cashier`

Important:

- seeded system users do not carry a shared default password in source control
- seeded credentials must be initialized explicitly outside seed data
- do not document or commit local bootstrap passwords
- `ResetUserPassword` currently blocks system users, so if system-user bootstrap is needed it should use a separate explicit setup path instead of relying on a hidden default credential

## Secret handling

JWT secrets must not be committed in `appsettings*.json`.

Use one of these local development approaches instead:

- .NET user secrets
- environment variables

The app is expected to fail fast when required JWT configuration is missing.

Example local setup with user secrets:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<your-local-dev-key>" \
  --project Host/WebApi/Commerce.Host.WebApi/Commerce.Host.WebApi.csproj
```

## API conventions

`Identity` follows the shared API response contract used by controller-based modules:

- success item/detail responses return `ApiResponse<T>` serialized as `snake_case`
- list responses return `PagedApiResponse<T>` serialized as `snake_case`
- handled application errors return `ErrorResponse` with `status`, `error_code`, and `message`
- list query params use explicit `snake_case` names such as `page`, `page_size`, `search`, `sort_by`, and `desc`

Logout note: revoking a refresh token does not invalidate already-issued access tokens before their natural expiry.

## CQRS snapshot

Identity uses MediatR `IRequest<T>` handlers for auth flows and management APIs.

- controllers bind directly to command/query models and delegate with `_mediator.Send(...)`
- command/query inputs use `sealed record` property-based models
- route parameters stay in the controller and are merged into commands with `with` when needed
- handlers own the application logic for auth, users, roles, permissions, and authorization overview
- FluentValidation is applied through MediatR pipeline behaviors
