# Backend — Agent Guide

ASP.NET Core Web API (.NET 9), clean architecture, ~25 controllers / ~35 domain
entities. See root `../AGENTS.md` for the whole-repo picture and deploy targets.

## Architecture

### Layers and dependency direction

```
TrojesDeMaranon.Api              controllers, Program.cs, middleware, appsettings*.json
        │  depends on
        ├── TrojesDeMaranon.Infrastructure   JWT token service, auth plumbing
        ├── TrojesDeMaranon.Persistence      EF Core DbContext, migrations, seed data
        │       │  depends on
        └── TrojesDeMaranon.Application      DTOs, FluentValidation, MediatR commands/queries + handlers
                │  depends on
                TrojesDeMaranon.Domain       entities only — no dependencies on any other project
```

Rule of thumb: `Domain` knows about nothing else. `Application` knows about `Domain`
only (defines `IAppDbContext` as an interface it owns). `Persistence` and
`Infrastructure` implement Application's interfaces. `Api` wires everything together in
`Program.cs` and never contains business logic — controllers are thin dispatchers.

### Request lifecycle (CQRS via MediatR)

```
HTTP request
  → ExceptionHandlingMiddleware   (catches unhandled exceptions → error envelope)
  → Serilog request logging
  → CORS
  → Authentication (JWT bearer)
  → Authorization
  → Controller action
      - reads CompanyId from ICurrentUserService (JWT claims)
      - builds a Command/Query record, calls mediator.Send(...)
  → Application handler (IRequestHandler<TRequest, ApiResponse<T>>)
      - FluentValidation validates the request DTO (commands only)
      - reads/writes via IAppDbContext (EF Core), always filtered by CompanyId
      - maps entity ⇄ DTO
  → AppDbContext.SaveChangesAsync (writes only)
      - auto-applies audit fields + writes an AuditLog row
      - soft-delete: EntityState.Deleted is rewritten to IsDeleted = true
  → handler returns ApiResponse<T> { success, data, message, errors }
  → controller.ToActionResult(...) → 200/201/400/404 with the same envelope
```

Every controller follows this shape — see `ClientsController` +
`Application/Clients/ClientHandlers.cs` for the clearest example to copy for a new
resource. There is no separate service layer: Application handlers talk to
`IAppDbContext` directly.

### Domain model (bounded areas)

The ~35 entities in `AppDbContext` group into four areas, which map to the MVPs already
shipped (see root README and `docs/`):

| Area | Key entities | Notes |
|---|---|---|
| **Identity & tenancy** | `Company`, `Branch`, `CompanySettings`, `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, `RefreshToken`, `AuditLog` | `Company` is the tenant root; almost every other entity implements `ICompanyScoped` and is filtered by `CompanyId`. Auth/RBAC lives here. |
| **Catalogs (master data)** | `Client`, `Supplier`, `Unit`, `MaterialFamily`/`MaterialSubfamily`, `Material`, `MaterialUnitConversion`, `Warehouse`, `ActivityCatalog` | Reference data other modules point at by FK (`Restrict` delete behavior — catalogs can't be deleted while referenced). |
| **Projects & platforms** | `Project` → `Platform` → `PlatformActivity`, `EstimatedMaterialConsumption` | A `Project` has many `Platform`s (construction stages/areas); each `Platform` has planned `Activities` and estimated material consumption used later to compare against real usage. |
| **Inventory (documental + real)** | `MaterialReceipt`/`MaterialIssue`/`InventoryAdjustment`/`InventoryTransfer` (+ their `*Line` children), `InventoryMovement`, `InventoryBalance` | The four document types are the *transactions* (receive, issue, adjust, transfer); each posts one or more `InventoryMovement` rows and updates the `InventoryBalance` per warehouse+material. This is the real-consumption counterpart to the estimated consumption above. |

All of the above are `AuditableEntity` (soft delete + audit fields) except pure join
tables (`UserRole`, `RolePermission`) and `AuditLog` itself.

## Database: Supabase Postgres via Npgsql

- Provider: `Npgsql.EntityFrameworkCore.PostgreSQL` (switched from SQL Server — the
  model no longer uses `IsRowVersion()`/SQL Server rowversion; there is currently **no
  optimistic-concurrency token** at all, since Npgsql 9.0.4 has no
  `UseXminAsConcurrencyToken` and nothing in the code relied on the old token).
- Connection string: `ConnectionStrings:DefaultConnection` in `appsettings.json`
  (placeholder password) — for local dev, copy
  `src/TrojesDeMaranon.Api/appsettings.Local.example.json` to
  `appsettings.Local.json` (gitignored) and put the real Supabase password there.
  In deployed environments, override via env var
  `ConnectionStrings__DefaultConnection`.
- Supabase host uses the **transaction pooler** (port 6543) by default; this works for
  both migrations and app runtime in this project. Fall back to the session/direct port
  (5432) only if you hit prepared-statement errors from `dotnet ef` commands.
- On startup, `Program.cs` runs `Database.MigrateAsync()` then `DatabaseSeeder.SeedAsync`
  automatically — no manual seed step needed after `dotnet run`.
- Seed admin login: `admin@trojes.demo` / `Admin123!`.

Common commands (run from `backend/`):

```bash
dotnet build TrojesDeMaranon.sln
dotnet ef database update --project src/TrojesDeMaranon.Persistence --startup-project src/TrojesDeMaranon.Api
dotnet ef migrations add <Name> --project src/TrojesDeMaranon.Persistence --startup-project src/TrojesDeMaranon.Api
dotnet run --project src/TrojesDeMaranon.Api        # http://localhost:5000, Swagger at /swagger
```

## Auth

JWT + refresh tokens. `Jwt:SecretKey`/`Issuer`/`Audience` in appsettings; refresh tokens
are persisted (`RefreshToken` entity) and rotated via `POST /api/v1/auth/refresh-token`.
`ICurrentUserService`/`CurrentUserService` resolve the current user from the JWT claims
for company-scoping and audit fields.

## Cross-cutting patterns already in place — reuse, don't reinvent

- **Soft delete**: entities inherit `AuditableEntity` (`IsDeleted`, `DeletedAt`,
  `DeletedByUserId`); `AppDbContext` applies `HasQueryFilter(x => !x.IsDeleted)` per
  entity and turns `EntityState.Deleted` into a soft-delete on `SaveChangesAsync`.
- **Audit log**: `AppDbContext.ApplyAudit()` auto-writes an `AuditLog` row for every
  tracked `AuditableEntity` change — don't write manual audit-logging code in handlers.
- **Multi-tenancy**: most entities implement `ICompanyScoped` (`CompanyId`); filter by
  company in queries, don't add a new scoping mechanism.
- **Response envelope**: controllers return a consistent `{ success, data, message,
  errors }` shape via `ControllerResponseExtensions.cs` — reuse it, don't hand-roll
  responses.
- **Validation**: FluentValidation validators per request DTO, called explicitly inside
  command handlers (`validator.ValidateAndThrowAsync(...)`) — there is no MediatR
  validation pipeline behavior, validation is not automatic for every request.
- **Errors**: global handling via `ExceptionHandlingMiddleware`.

## Deploy: Google Cloud Run

Containerized (`Dockerfile`, multi-stage, binds `$PORT`). Full deploy steps and the
`gcloud run deploy` invocation are in `README-deploy.md`. gcloud is configured
(project `trojes-de-maranon`, account `fernando.krauss.f@gmail.com`, via a *named*
gcloud config so it doesn't clobber the machine's default project) but **nothing has
been deployed yet** — treat any deploy action as something to confirm before running.

## Testing

There is no test project in this solution yet. Verify changes by building
(`dotnet build`) and exercising the relevant endpoint (`dotnet run` + `curl`/Swagger),
not by assuming test coverage exists.
