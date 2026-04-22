`# Identity Module

`Modules/Identity` owns the authorization schema for the modular monolith.

## Current snapshot

Use this section as the quick-load context for AI assistants.

### What exists now

- JWT login endpoint exists at `POST /api/identity/login`
- JWT bearer authentication is configured in the host
- JWT signing secrets must be supplied from environment variables or secret storage, not committed `appsettings*.json`
- current user resolution is claims-based through `ICurrentUser`
- permission enforcement runs through custom middleware, not ASP.NET Core `[Authorize]`
- runtime authorization uses the database-backed `DbPermissionGate`
- authorization entities exist: `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, `UserPermission`
- seed users exist: `admin`, `manager`, `cashier`
- seed roles exist: `ADMIN`, `MANAGER`, `CASHIER`
- identity read endpoints currently exist for users and authorization overview

### Current request pipeline

1. `UseAuthentication()` validates JWT and populates `HttpContext.User`
2. `UseFeatureGate()` reads endpoint metadata
3. if an endpoint has `[RequireModule]`, the middleware checks module availability and returns `404` when disabled
4. if an endpoint has `[RequireFeature]`, the middleware checks feature availability and returns `404` when disabled
5. if an endpoint has `[RequirePermission]`, the middleware checks authentication first, then permission access
6. unauthenticated access to a permissioned endpoint returns `401`
7. authenticated access without permission returns `403`

Important: `POST /api/identity/login` is currently not decorated with `[RequireModule("Identity")]`, so the full module-gating path does not apply uniformly to every Identity endpoint.

### Current user resolution

`ICurrentUser` currently exposes:

- `UserId`
- `UserName`
- `IsAuthenticated`

`HeaderCurrentUserAccessor` resolves user data from claims first:

For `UserId`:
- `ClaimTypes.NameIdentifier`
- `sub`

For `UserName`:
- `preferred_username`
- `sub`
- `ClaimTypes.NameIdentifier`

In development only, it can fall back to headers:

- `X-Commerce-UserId` (stable `User.Id` GUID)
- `X-Commerce-UserName`

Important: `ICurrentUser.UserId` is intended to represent the stable persisted `User.Id`, while `UserName` carries the login name.

### Current permission source

The active permission source is `DbPermissionGate`.

Resolution order:

1. user direct deny
2. user direct allow
3. role deny
4. role allow
5. default deny

### API coverage now

Implemented:

- `POST /api/identity/login`
- `GET /api/identity/users`
- `GET /api/identity/me`
- `GET /api/identity/my-permissions`
- `GET /api/identity/authorization/overview`
- `GET /api/identity/health`

Not implemented yet:

- user management write endpoints
- role management endpoints
- permission management endpoints
- role assignment endpoints
- user direct permission assignment endpoints

### Important design note

Current authorization is metadata-driven with custom attributes such as `[RequirePermission(...)]`. It does not currently rely on ASP.NET Core authorization policies or `[Authorize]` on the module controllers.

### Suggested use for future AI sessions

When extending Identity, assume the current system already has:

- JWT issuance
- claims-based current user access
- DB-backed permission evaluation
- module/feature/permission gate middleware

When planning new work, treat `/me`, `/my-permissions`, and full user/role/permission management APIs as missing scope to be added.


## Scope

- authorization entities live in `Modules/Identity`
- `CommerceCore` stays limited to shared foundation and abstractions
- persistence reuses the existing `BaseDbContext`, `IRepository<TEntity>`, `BaseRepository<TEntity>`, `IUnitOfWork`, and `UnitOfWork`
- database wiring comes from the host via `ConnectionStrings:Default`

## Database wiring

The system currently assumes `1 deployment = 1 database`.

`IdentityDbContext` is registered by the host using the shared `ConnectionStrings:Default` value from `Host/WebApi/Commerce.Host.WebApi/appsettings.json`. The shared default database wiring currently uses PostgreSQL via `UseNpgsql`. The module does not read connection strings from configuration directly and does not own provider selection.

## Authorization schema

The module currently defines:

- aggregate roots: `User`, `Role`, `Permission`
- association entities: `UserRole`, `RolePermission`, `UserPermission`
- seed roles: `ADMIN`, `MANAGER`, `CASHIER`
- seed permissions following `{Module}.{FeatureOrArea}.{Action}`
- identity domain read permissions:
  - `Identity.Users.Read`
  - `Identity.Authorization.Read`

## Resolution intent

Authorization is intended to resolve in this order:

1. user direct deny
2. user direct allow
3. role deny
4. role allow
5. default deny

Module gating and feature gating still run before authorization. The active runtime permission source is the database-backed `DbPermissionGate`.

## API conventions

`Identity` now follows the shared API response contract used by controller-based modules:

- success item/detail responses return `ApiResponse<T>` serialized as `snake_case`
- list responses return `PagedApiResponse<T>` serialized as `snake_case`
- handled application errors return `ErrorResponse` with `status`, `error_code`, and `message`
- list query params use explicit `snake_case` names such as `page`, `page_size`, `search`, `sort_by`, and `desc`
- login validation failures use `ValidationAppException`
- invalid credentials use `UnauthorizedAppException`

Current controller coverage:

- `POST /api/identity/login` returns `ApiResponse<LoginResponse>`
- `GET /api/identity/users` returns `PagedApiResponse<T>` and requires `Identity.Users.Read`
- `GET /api/identity/authorization/overview` returns `ApiResponse<T>` and requires `Identity.Authorization.Read`
- `GET /api/identity/health` returns `ApiResponse<T>`

## CQRS snapshot

Identity now pilots CQRS with MediatR `IRequest<T>` for login and the current read endpoints.

- controllers bind directly to command/query models and delegate with `_mediator.Send(...)`
- command/query inputs use `sealed record` property-based models
- route parameters stay in the controller and are merged into commands with `with` when needed
- handlers own the application logic for login, users listing, and authorization overview
- `PagedListRequest` is the shared base model for list queries
