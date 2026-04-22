# CmsModulor Backend Skeleton

Backend skeleton for a .NET 8 modular monolith e-commerce system.

## Root structure

- `CommerceCore`: shared cross-cutting building blocks for the whole backend.
- `Modules`: business modules that depend only on `CommerceCore`.
- `Host`: ASP.NET Core Web API startup application.

## CommerceCore projects

- `CommerceCore.SharedKernel`: primitives and shared abstractions.
- `CommerceCore.Application`: application-layer contracts.
- `CommerceCore.Infrastructure`: reusable infrastructure services.
- `CommerceCore.FeatureManagement`: module and feature toggling support.
- `CommerceCore.Observability`: logging, tracing, metrics, audit, and health-check plumbing.

## Module shape

Each module follows a consistent package layout:

- `Domain`
- `Application`
- `Infrastructure`
- `Controllers`
- `Contracts`

Each module exposes:

- `Add{Module}Module(IServiceCollection, IConfiguration, string connectionString)` for modules that own a `DbContext`
- `Map{Module}Endpoints(IEndpointRouteBuilder)`

`Identity` and `Payment` now use ASP.NET MVC controllers with `[ApiController]` and controller-based routing. Other modules still expose Minimal API endpoint mappings for now.

## Host

`Host/WebApi/Commerce.Host.WebApi` wires shared services, exposes Swagger, and loads modules from `Host/WebApi/Commerce.Host.WebApi/appsettings.json`.

The host is also the only place allowed to read database connection strings. The main configuration path is `Host/WebApi/Commerce.Host.WebApi/appsettings.json`, and the shared connection string is stored under `ConnectionStrings:Default`.

## API response convention

HTTP responses now follow a shared envelope contract across the host pipeline and controller-based modules:

- success item/detail/update/delete responses return:
  - `status`
  - `data`
- create responses return the same envelope with `status = 201`
- list responses return:
  - `status`
  - `data`
  - `pagination.total`
  - `pagination.page`
  - `pagination.pageSize`
- handled application errors return:
  - `status`
  - `errorCode`
  - `message`

The shared response models live in `CommerceCore.Application/Responses`, and the shared exception hierarchy lives in `CommerceCore.SharedKernel/Exceptions`.

## Exception mapping

The host registers a global exception middleware that maps shared application exceptions to HTTP status codes:

1. `ValidationAppException` -> `400`
2. `UnauthorizedAppException` -> `401`
3. `ForbiddenAppException` -> `403`
4. `NotFoundAppException` -> `404`
5. `ConflictAppException` -> `409`
6. `BusinessRuleAppException` -> `422`
7. unhandled exceptions -> `500`

`[ApiController]` model validation is also normalized to the same `ErrorResponse` contract with error code `1001`.

## Module loading

Module loading is configuration-driven through `Modules:Enabled` in `Host/WebApi/Commerce.Host.WebApi/appsettings.json`.

A disabled module is skipped during service registration and its endpoints are not mapped into the ASP.NET Core pipeline. The host also exposes `GET /api/modules` so you can inspect current module states at runtime.

## Feature gating

Feature flags are read from `Host/WebApi/Commerce.Host.WebApi/appsettings.json` under `Features:Flags`.

`UseFeatureGate()` inspects endpoint metadata for `RequireFeatureAttribute`. If a required feature is disabled, the request is short-circuited with `404` and a JSON payload describing the disabled feature. This keeps feature enforcement on the backend even when the owning module is enabled.

## Permission gating

Permission checks are enforced through endpoint metadata in `CommerceCore.FeatureManagement`, but runtime permission resolution now reads from the authorization schema in `Modules/Identity`.

Requests are evaluated in order: module, feature, then permission. A disabled module returns `404`, a disabled feature returns `404`, an anonymous caller hitting a permission-protected endpoint returns `401`, and an authenticated caller without the required permission returns `403`.

For local and development testing, the current user is resolved from request headers:

- `X-Commerce-UserId`
- `X-Commerce-UserName`

For the current development flow, `X-Commerce-UserId` is treated as username semantics and resolved against `User.NormalizedUserName` in the `Identity` database schema.

Permission resolution order is:

1. user direct deny
2. user direct allow
3. role deny
4. role allow
5. default deny

The active runtime permission source is `Modules/Identity/Commerce.Modules.Identity/Infrastructure/Security/DbPermissionGate.cs`, which reads from the seeded `IdentityDbContext` authorization tables. The `Permissions:Users` section in `Host/WebApi/Commerce.Host.WebApi/appsettings.json` may still exist for local reference, but it is no longer used at runtime.

When an endpoint requires authorization metadata, prefer attaching `RequirePermission(...)` directly on the endpoint mapping so the route, feature requirement, and permission requirement stay visible in one fluent chain. Group-level permission metadata should only be used when every endpoint in the group shares the same permission and the mapping still reads clearly.

## Persistence foundation

`CommerceCore` now provides a lightweight persistence foundation for future business modules:

- audit abstractions for `CreatedAtUtc`, `CreatedBy`, `UpdatedAtUtc`, and `UpdatedBy`
- soft delete abstraction with `IsDeleted`, `DeletedAtUtc`, and `DeletedBy`
- `BaseDbContext` in `CommerceCore.Infrastructure` to apply audit and soft delete rules in `SaveChangesAsync`
- global query filters for soft-deleted entities
- restrictive delete behavior by default
- seed conventions through model builder extensions instead of seeding in `Program.cs`
- shared `UseDefaultDatabase(connectionString)` helper for provider wiring

## Database wiring

The current deployment model is `1 deployment = 1 database`.

Multiple modules can own separate `DbContext` types, but they all share the same `ConnectionStrings:Default` value from `Host/WebApi/Commerce.Host.WebApi/appsettings.json`.

Local and development environments currently use PostgreSQL via `ConnectionStrings:Default`, with a default local shape of `Host=localhost;Port=5432;Database=commerce_db;Username=thaind;Password=;`.

Modules must not read `GetConnectionString(...)` directly and must not hardcode provider/connection string setup. The host reads the connection string once during startup and passes it into module registration, while `CommerceCore` centralizes provider wiring through `UseDefaultDatabase(connectionString)`.

This keeps `CommerceCore` focused on shared persistence foundation while modules continue to own business entities and module-specific `DbContext` implementations.

## Identity authorization schema

Authorization entities now live in `Modules/Identity` instead of `CommerceCore`, and the active permission resolver also lives there.

`CommerceCore` remains responsible only for reusable persistence foundation and shared abstractions, while `Modules/Identity` reuses that foundation to define the authorization schema with `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, and `UserPermission`.

Module gating and feature gating still execute before authorization, and the active permission resolution order is: user direct deny, user direct allow, role deny, role allow, then default deny.
