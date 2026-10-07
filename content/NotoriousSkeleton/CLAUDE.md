# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

NotoriousSkeleton is a skeleton project derived from Lanae. The repository is a mono-repo containing a .NET 10 backend API (modular monolith, two example modules: `Catalog` and `Ordering`), a minimal React front-end, shared .NET libraries, and deployment config. The solution file is `NotoriousSkeleton.slnx` (the newer XML solution format).

Top-level numbered folders (the numbering is meaningful and mirrors the solution folder structure):
- `1. Internal API/` — ASP.NET Core backend (.NET 10)
- `2. Front/notoriousskeleton.front/` — minimal React + TypeScript + Vite app (served by the API in production)
- `3. Libraries/` — shared .NET libraries (`NotoriousSkeleton.Libraries.*`)
- `5. Scripts/` — PowerShell scripts for EF Core migrations

Note: folder names contain spaces (e.g. `1. Internal API`). Always quote paths.

## Commands

### Backend (.NET, run from repo root)
`global.json` opts `dotnet test` into **Microsoft.Testing.Platform** (required by xUnit v3 on the .NET 10 SDK): use `--project`, not a positional path.
```bash
dotnet build NotoriousSkeleton.slnx
dotnet test --project "1. Internal API/NotoriousSkeleton.Catalog.Unit.Tests"          # unit tests
dotnet test --project "1. Internal API/NotoriousSkeleton.Catalog.Integration.Tests"   # integration tests (Docker required)
dotnet test --project "1. Internal API/NotoriousSkeleton.Catalog.Unit.Tests" --filter-class "*AddProductCommandFeatureUnitTests"  # single class
dotnet test --project "1. Internal API/NotoriousSkeleton.Catalog.Unit.Tests" --filter-method "*AddProduct_ShouldReturnError*"     # single test
```
Test projects: `NotoriousSkeleton.{Catalog,Ordering}.{Unit,Integration}.Tests`, `NotoriousSkeleton.SharedKernel.Unit.Tests`.

### Front-end (run from `2. Front/notoriousskeleton.front/`)
```bash
npm run dev      # Vite dev server; proxies /api -> http://localhost:8080
npm run build    # tsc -b && vite build
npm run lint     # eslint
```
There is no front-end test runner configured.

### Running the full stack
- `docker-compose.yml` (+ `docker-compose.override.yml`) runs the API + SQL Server (`mcr.microsoft.com/mssql/server:2022-latest`, `sa` / `Skeleton@Passw0rd`, port 1433). In Visual Studio, `docker-compose.dcproj` launches it ("Docker Compose" profile).
- The database is not migrated on startup: run `5. Scripts/Update-Database.ps1 -Module All` once SQL Server is up.
- Integration tests spin up their own SQL Server containers via NotoriousTest/Testcontainers — Docker must be running to run them.

### EF Core migrations (PowerShell, from repo root)
```powershell
& "5. Scripts/Add-ModuleMigration.ps1" <MigrationName> -Module <Catalog|Ordering|All>   # dotnet ef migrations add, with the right --context
& "5. Scripts/Update-Database.ps1" -Module <Catalog|Ordering|All>                        # applies migrations to the dev DB (localhost,1433)
& "5. Scripts/Create-SQLMigrationsFile.ps1"                                              # generates 1. Internal API/migrations.sql (idempotent script used at deploy)
```
Startup project is `NotoriousSkeleton.API`. Each module has its own `DbContext` (`CatalogDbContext`, `OrderingDbContext`) in its `Persistence.EntityFramework` project, its own schema (`catalog`, `ordering`) and its own `__EFMigrationsHistory` table in that schema. The module → project/context list lives in `5. Scripts/Modules.ps1`.

## Backend architecture

The backend follows a Clean/Onion architecture organized as a **modular monolith**. `Catalog` and `Ordering` are the two example bounded contexts. Dependencies point inward toward Domain.

Layers (per module, e.g. Catalog):
- **`NotoriousSkeleton.Catalog.Domain`** — aggregates and value objects (`Product/Product.cs`, `Product/Price.cs`). Pure, no infrastructure deps. Organized by concept.
- **`NotoriousSkeleton.Catalog.Application`** — use cases. Each use case is a **Feature class** with a single `Invoke(...)` method (this project's CQRS-style handler, not MediatR). Layout:
  - `Commands/<UseCase>/` — `<UseCase>CommandFeature`, `<UseCase>CommandParameters`, `<UseCase>Errors`;
  - `Queries/<UseCase>/` — `<UseCase>QueryFeature`, plus `Providers/` (its readonly repository interface) and `ReadModels/`;
  - `Providers/` — interfaces used by commands (repositories such as `IProductRepository`, `IUserProvider`, cross-module providers such as Ordering's `IProductProvider`);
  - `Authentication/User.cs`, `Common/AuthenticatedFeature.cs`, `<Module>Errors.cs` (module-wide errors).
- **Infrastructure adapters** — one project per concern, each implementing Application interfaces:
  - `Persistence.EntityFramework` (SQL Server, EF Core)
  - `Authentication.Auth0` (`IUserProvider`) — one adapter per module (`AddCatalogUserProvider()`, `AddOrderingUserProvider()`)
  - `Client.<OtherModule>` — e.g. `Ordering.Client.Catalog`, implementing Ordering's `IProductProvider` on top of `Catalog.Externals`

  Adapter implementation classes are **`internal`**. Each infra project exposes its internals only to its paired **`.Extensions`** project (which holds the `AddX()` DI registration, the single public surface) and to the module's `Integration.Tests`, via `InternalsVisibleTo` in the `.csproj`. So consumers depend on interfaces + call the `.Extensions` registration; they cannot reference concrete adapters. `Domain` likewise grants `InternalsVisibleTo` to `Persistence.EntityFramework` and to the integration tests.
- **`NotoriousSkeleton.Catalog`** — the module's API surface: MVC Controllers + `ModuleExtensions.AddCatalogModule()` which composes all sub-registrations, + `Clients/` implementing the module's `Externals` contracts for other modules.
- **`NotoriousSkeleton.Catalog.Externals`** — contracts other modules may depend on (`IProductClient`, `Product` record). Never reference another module's Application/Domain directly.
- **`NotoriousSkeleton.API`** — thin ASP.NET Core host: `Program.cs`, global exception handler, JSON/value-object serializers, API versioning, rate limiting, Serilog, static files (the built front in `wwwroot`).

### Key conventions
- **Result pattern**: `NotoriousSkeleton.Libraries.Result.Result` / `Result<T>` is used instead of throwing for expected failures. Features validate their inputs and return accumulated `Error`s; `Error`, `List<Error>`, and `T` all implicitly convert to `Result<T>`, so features `return errors;` or `return value;` directly. Check `.HasErrors` / `.IsSuccess` before using `.Value`.
- **Domain aggregates & value objects validate in their constructor** and throw `DomainException` on invalid input (see `Product`, `Price`, `Order`, `Email`). There is no `Result`-returning factory nor a separate `Rehydrate`: persistence reconstructs aggregates through the same public constructor, so invalid DB state fails fast. User-facing validation lives in the command Feature (which returns structured `Error`s) *before* the aggregate is constructed; the constructor is the last-resort guard.
- **Strongly-typed IDs & value objects**: IDs are `record struct`s (e.g. `ProductId(int Value)`, `OrderId(Guid Value)`) with implicit conversions; quantities such as `Price` are value objects implementing `IValueObject<T>`. The API layer registers custom JSON converters (`JsonValueObjectConverterFactory`) so these serialize as primitives.
- **Authenticated features** inherit `AuthenticatedFeature` (`Application/Common`), which exposes `User` via `IUserProvider` and `EnsureAuthenticated()`. Call `EnsureAuthenticated()` at the top of `Invoke`, then use `User.Id`.
- **Feature registration**: every Feature must be added (`AddScoped<TFeature>()`) in `ServiceCollectionExtensions.Add<Module>Features()` (`Application.DependencyInjection.Microsoft`). Controllers inject Feature classes directly.
- **Reads for display go through a per-use-case readonly repository → `ReadModel`. This is the standard to apply to every query feature.** The interface is:
  - **named** `I<UseCase>ReadonlyRepository` (e.g. `IGetAllProductsReadonlyRepository`, `IGetOrderByIdReadonlyRepository`) — one interface per query feature, one method on it, returning that feature's `ReadModel`;
  - **located** in the Application layer, in the feature's own `Providers/` folder (`Queries/<UseCase>/Providers/`) — *not* in Domain;
  - **implemented** by an `internal EF<X>ReadonlyRepository` that projects **straight from EF entities to the `ReadModel`** (`.Select(e => e.ToReadModel())`, `AsNoTracking()`) — no domain aggregate is materialized. Do not route a display read through the Domain or return an aggregate to a controller.
- **Only commands touch the Domain.** Writes use a repository declared in `Application/Providers` (e.g. `IProductRepository.Save(aggregate)`, `IOrderRepository.Save(aggregate)`). When a command needs to read data for validation/mapping, it uses a **`Provider`** returning domain objects (e.g. Ordering's `IProductProvider.GetByIds(ids)`, implemented by the `Client.Catalog` adapter). This path exists only to serve domain logic inside commands; it is **not** the pattern for screen reads.
- Entity ↔ domain mapping uses the `IMappable<TModel, TEntity>` interface (`ToModel()` / static `FromModel()`); entity → read model uses a plain `ToReadModel()` projection.
- **Errors** are defined as static `Error` constants per use case / module (e.g. `AddProductErrors`, `PlaceOrderErrors`, `OrderingErrors`) rather than inline strings.
- Central package versioning: all NuGet versions are pinned in `Directory.Packages.props` (`ManagePackageVersionsCentrally`). Add versions there, not in individual `.csproj` files. `.csproj` uses `<PackageReference Include="..." />` with no `Version`.
- Log messages in this codebase are written in **English** — match that when adding logs.

### Authentication
The app authenticates via JWT (Auth0, `ClientCredentialAuthentication` section). Controllers use `[Authorize]`; the current user is surfaced to Application through each module's `IUserProvider`.

## Testing

- **Unit tests** (`NotoriousSkeleton.<Module>.Unit.Tests`): xUnit v3 + FakeItEasy, `Features/` (one file per Feature, testing `Invoke` in isolation with faked repositories/providers) and `Domain/` (aggregate/value-object invariants).
- **Integration tests** (`NotoriousSkeleton.<Module>.Integration.Tests`): built on **NotoriousTest 5** (`NotoriousTest.XUnit`, `.Web`, `.SqlServer`) + Testcontainers (real SQL Server). Structure:
  - `Environment/NotoriousSkeleton<Module>Environment` — `EnvironmentBase` adding the database infrastructure(s) + `this.AddWebApplication<InternalAPIApplication>()`. The Ordering environment also starts the Catalog database, since PlaceOrder reads products through the Catalog externals.
  - `Infrastructure/` — `NotoriousSkeleton<Module>Database` (`SqlServerContainerInfrastructure`: publishes `ConnectionStrings:<Module>`, applies EF migrations in `Initialize`, Respawn resets the module schema after each test), `InternalAPIApplication` (`WebApplication<Program>` over the real `Program`), `TestAuthHandler` (bypasses real auth; fixed `TestAuthHandler.UserId`).
  - `Framework/` — Arrange/Act/Assert helper classes (`ArrangeFramework`, `ActFramework`, `AssertFramework`, `Ordering*` variants). Tests inherit `IntegrationTest<TEnvironment>` (ctor takes `XUnitFixture<TEnvironment>`) and read as arrange → act → assert against these helpers, hitting the real HTTP pipeline.
  - Custom media-type formatting (`ValueObjectAwareMediaTypeFormatter`) so value objects (de)serialize correctly over HTTP in tests.
  - Requires Docker running. NotoriousTest's DoggyDog watchdog runs next to the test process to clean containers after a crash.

## Front-end architecture (`2. Front/notoriousskeleton.front`)

React 19 + TypeScript + Vite, kept minimal (no router, auth or design system yet). `src/` uses the same numbered-folder scheme as the repo root, with a path alias per folder (see `vite.config.ts` / `tsconfig.app.json`):
- **`src/1. UI/`** (`@UI`) — the app itself: `app/`, `configuration/` (runtime configuration loaded from `public/configuration/configuration.json`, whose `$Build__*` placeholders are substituted by `1. Internal API/entrypoint.sh` at container start).
- **`src/2. business/`** (`@business`) — framework-agnostic domain models and React context providers.
- **`src/3. data/`** (`@data`) — one `*.api.ts` file per endpoint. API calls go to `/api/...` (proxied to the backend in dev via `vite.config.ts`).
- **`src/4. common/`** (`@common`) — cross-cutting helpers: the shared `fetch` wrapper (`api/fetch.ts`) that throws on non-OK responses.

## CI/CD

GitHub Actions in `.github/workflows/internal-api-deployment.yml`: build & test (front build, API build, every `*.Tests` project including integration tests) on PR/push to `develop`; then build & push the `Dockerfile.prod` image (front + API) to `ghcr.io/<owner>/notoriousskeleton-api`; then deploy to the DEV server over SSH: copy `docker-compose.api.yml`, `.env.internal-api` and `migrations.sql`, apply `migrations.sql` with `sqlcmd`, and `docker compose up` with secrets resolved by 1Password (`op://NotoriousSkeleton-$ENV/...`). Required secrets: `SERVER_IP`, `SERVER_USER_NAME`, `SERVER_USER_KEY`, `OP_SERVICE_ACCOUNT_TOKEN`. `develop` is the main branch.

## Skills

- `.claude/skills/scaffold-module` — generates a whole new module (8 projects), adds it to the `.slnx` and to `5. Scripts/Modules.ps1`.
- `.claude/skills/scaffold-feature` — generates a Query or Command feature inside a module (`-Module <Module>`).
