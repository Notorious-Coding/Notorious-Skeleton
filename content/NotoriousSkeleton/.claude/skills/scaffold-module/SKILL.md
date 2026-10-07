---
name: scaffold-module
description: >-
  Scaffold an entire new backend module (bounded context) in the NotoriousSkeleton
  .NET modular monolith, mirroring the Catalog/Ordering modules' Clean/Onion project
  layout. Use this whenever the user wants to create/add a whole new module, bounded
  context, or vertical slice on the backend — e.g. "new module", "scaffold a
  module", "bootstrap the Billing/Shipping/Inventory module", "add a new bounded
  context" — even if they don't say the word "scaffold". Generates the 8 core
  projects (Domain, Application, Application.DI.Microsoft, Persistence.EntityFramework
  (+ .Extensions), the <Module> API-surface project, Unit.Tests, Integration.Tests)
  with correct references/namespaces/DI skeletons, adds them to NotoriousSkeleton.slnx,
  and lists the host-wiring edits. For adding a single feature/use case INSIDE an
  existing module, use scaffold-feature instead.
---

# Scaffold a new backend module

NotoriousSkeleton's backend is a **modular monolith**: each bounded context
(`Catalog`, `Ordering`, …) is a set of ~8 projects under `1. Internal API/`
following a strict Clean/Onion layering — Domain ← Application ← Infrastructure,
composed by a thin `NotoriousSkeleton.<Module>` API-surface project and hosted by
`NotoriousSkeleton.API`. Standing up all of that by hand is tedious and easy to get
subtly wrong (project references, `InternalsVisibleTo`, the `internal` DbContext,
the DI `AddXModule`/`AddXControllers` entry points, solution membership).

This skill generates the whole skeleton deterministically so the layering,
namespaces, and registrations stay consistent with Catalog, then you do a few
contextual host edits and start adding features.

See the repo `CLAUDE.md` "Backend architecture" section for the full rationale —
this skill assumes it.

## Step 1 — Gather the inputs

Ask the user (or infer from their request) only what you need:

- **Module**: the bounded-context name, PascalCase, singular — e.g. `Billing`,
  `Shipping`, `Inventory`. Becomes the `<Module>` segment in every project name and
  namespace (`NotoriousSkeleton.<Module>.*`).
- **Schema** (optional): the SQL Server schema the module's `DbContext` uses.
  Defaults to the lower-cased module name.
- **SolutionFolder** (optional): the label for the module's folder in
  `NotoriousSkeleton.slnx`. Defaults to the module name; pass the numbered form to
  match the convention (Catalog sits under `1.1 Catalog`, Ordering under
  `1.2 Ordering`), e.g. `1.4 Billing`.

If the user is vague, pick sensible defaults and say what you chose — don't stall.

## Step 2 — Run the generator

The script only ever **creates** files (safe, idempotent — it skips any file that
already exists) and writes UTF-8 with BOM. Run it from the repo root:

```powershell
& ".claude/skills/scaffold-module/scripts/New-Module.ps1" -Module <Module> [-Schema <schema>] [-SolutionFolder "<label>"] [-AddToSolution:$false]
```

Example:

```powershell
& ".claude/skills/scaffold-module/scripts/New-Module.ps1" -Module Billing -SolutionFolder "1.4 Billing"
```

It creates the 8 projects, adds them to `NotoriousSkeleton.slnx` (nested under
`1. Internal API/<SolutionFolder>/…`, unless you pass `-AddToSolution:$false`),
registers the module in `5. Scripts/Modules.ps1`, then prints everything it
created and the wiring checklist below.

What it generates (all under `1. Internal API/`):

| Project | Role |
| --- | --- |
| `NotoriousSkeleton.<Module>.Domain` | aggregates, value objects. `InternalsVisibleTo` Persistence.EF + Integration.Tests |
| `NotoriousSkeleton.<Module>.Application` | `Commands/`, `Queries/`, `Providers/` use cases (see `scaffold-feature`) |
| `NotoriousSkeleton.<Module>.Application.DependencyInjection.Microsoft` | `Add<Module>Features()` (Feature registrations) |
| `NotoriousSkeleton.<Module>.Persistence.EntityFramework` | `internal <Module>DbContext` (schema set), EF entities, repos |
| `NotoriousSkeleton.<Module>.Persistence.EntityFramework.Extensions` | `Add<Module>Persistence(config)` (repo + DbContext DI; SQL Server, per-schema `__EFMigrationsHistory`) |
| `NotoriousSkeleton.<Module>` | API surface: `ModuleExtensions` (`Add<Module>Module`/`Add<Module>Controllers`), `AssemblyMarker`, `Controllers/` |
| `NotoriousSkeleton.<Module>.Unit.Tests` | xUnit v3 + FakeItEasy, refs Application |
| `NotoriousSkeleton.<Module>.Integration.Tests` | NotoriousTest 5 + Testcontainers (SQL Server), refs NotoriousSkeleton.API + Persistence.EF |

## Step 3 — Wire it into the host (edits to existing files)

The generator deliberately leaves these contextual edits to you. Make them with
the Edit tool, matching the surrounding style.

**1. Reference the module from the host** —
`1. Internal API/NotoriousSkeleton.API/NotoriousSkeleton.API.csproj` (next to the Catalog refs):

```xml
<ProjectReference Include="..\NotoriousSkeleton.<Module>\NotoriousSkeleton.<Module>.csproj" />
<ProjectReference Include="..\NotoriousSkeleton.<Module>.Persistence.EntityFramework.Extensions\NotoriousSkeleton.<Module>.Persistence.EntityFramework.Extensions.csproj" />
```

**2. Register the module & its controllers** —
`1. Internal API/NotoriousSkeleton.API/Program.cs`:

```csharp
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.Add<Module>Module(builder.Configuration);   // <-- add

builder.Services.AddControllers()
    .AddCatalogControllers()
    .Add<Module>Controllers();                               // <-- chain on
```

**3. Add the connection string** —
`1. Internal API/NotoriousSkeleton.API/appsettings.json` **and** `appsettings.Development.json`.
The `Add<Module>Persistence` skeleton reads `GetConnectionString("<Module>")`.
Modules share the same SQL Server instance and separate by schema, so Development
can reuse Catalog's server/database:

```json
"ConnectionStrings": {
  "Catalog": "Server=notoriousskeleton.database,1433;Database=notoriousskeleton;User Id=sa;Password=Skeleton@Passw0rd;TrustServerCertificate=True",
  "<Module>": "Server=notoriousskeleton.database,1433;Database=notoriousskeleton;User Id=sa;Password=Skeleton@Passw0rd;TrustServerCertificate=True"
}
```

Also add `ConnectionStrings__<Module>` to `1. Internal API/docker-compose.api.yml`
and `1. Internal API/.env.internal-api` for the deployment.

**4. Dockerfiles** — add the module's `.csproj` `COPY` lines (restore layer) in
`1. Internal API/NotoriousSkeleton.API/Dockerfile` **and** `Dockerfile.prod`,
next to the Catalog/Ordering blocks.

## Step 4 — Build

```bash
dotnet build NotoriousSkeleton.slnx
```

The generated skeleton compiles as-is (empty module). Now add features with the
**scaffold-feature** skill (`-Module <Module>`).

## Optional infrastructure adapters

The core scaffold wires only **Persistence**. If the module needs more, add the
adapter pair the same way Catalog does — an `internal` adapter project + a
paired `.Extensions` project holding the single public `AddX()` registration,
with `InternalsVisibleTo` from the adapter to its `.Extensions` and to
`Integration.Tests`. Then add the `.Extensions` `ProjectReference` to
`NotoriousSkeleton.<Module>.csproj` and call its `AddX()` inside `Add<Module>Module`:

- **Authentication** (`Authentication.Auth0` + `.Extensions`) — needed if the
  module has **authenticated features**: `AuthenticatedFeature` (`Application/Common`)
  resolves `IUserProvider` (`Application/Providers`), which the Auth0 adapter
  implements. Each module owns its own `IUserProvider`/`User`/`AuthenticatedFeature`
  and its own adapter, registered as `Add<Module>UserProvider()`. Model on
  `NotoriousSkeleton.Catalog.Authentication.Auth0`.
- **Reading another module** — the provider module exposes a contracts project
  (`NotoriousSkeleton.<Provider>.Externals`, client interface + DTO records) implemented
  in `NotoriousSkeleton.<Provider>/Clients/` on top of a query Feature; the consumer
  module gets a `NotoriousSkeleton.<Module>.Client.<Provider>` (+ `.Extensions`)
  adapter implementing one of its own `Application/Providers` interfaces. Model on
  `Catalog.Externals` + `Ordering.Client.Catalog`.

## Integration tests harness

The Integration.Tests project is created with the right references and package
set, but **not** the NotoriousTest harness. Copy and adapt it from
`NotoriousSkeleton.Catalog.Integration.Tests` when you write the module's first
integration test:

- `Environment/NotoriousSkeleton<Module>Environment` — `EnvironmentBase` adding the
  module's database infrastructure(s) + `this.AddWebApplication<InternalAPIApplication>()`.
- `Infrastructure/NotoriousSkeleton<Module>Database` — `SqlServerContainerInfrastructure`
  (`DbPrefix`, `TableToIgnore = ["__EFMigrationsHistory"]`, `SchemasToInclude = ["<schema>"]`,
  `ConnectionStringKey => "ConnectionStrings:<Module>"`, `MigrateAsync()` in `Initialize`).
- `Infrastructure/InternalAPIApplication` + `TestAuthHandler`, `Framework/` Arrange/Act/Assert,
  `testsettings.json` (copied to output).

## Migrations (follow-up, once the module has entities)

All modules share one SQL Server database but separate by schema — **including a
per-module `__EFMigrationsHistory` table**. The generated `Add<Module>Persistence`
sets `MigrationsHistoryTable("__EFMigrationsHistory", "<schema>")` for exactly
this, so each module's migration history is isolated in its own schema.

The `5. Scripts/*.ps1` migration helpers are **module-aware**: they read the
shared module list `5. Scripts/Modules.ps1` (module → persistence project +
`DbContext`) and pass the correct `--context` automatically. **The generator
registers the new module in `Modules.ps1` for you** — no manual edit needed there.

A brand-new module has no aggregates, entities, `DbSet`s, or migrations, so
nothing to migrate yet. Once it gains its first entity + `DbSet`:

```powershell
& "5. Scripts/Add-ModuleMigration.ps1" <Name> -Module <Module>   # add a migration (its own Migrations/ folder)
& "5. Scripts/Update-Database.ps1" -Module <Module>              # apply it to the dev DB
& "5. Scripts/Create-SQLMigrationsFile.ps1"                      # regenerate the combined idempotent deploy SQL
```

`Create-SQLMigrationsFile.ps1` concatenates every module's idempotent script into
a single `1. Internal API/migrations.sql` — each context creating its own schema
and history table, so the file is replayable in one pass at deploy (`sqlcmd -b -i`).

## Scope & non-goals

- Scaffolds the **project skeleton + DI entry points + solution membership**. It
  does **not** author Domain aggregates, EF entities, controllers, or migrations
  — use **scaffold-feature** per use case afterward, targeting the new module.
- Persistence is the only infra adapter wired by default; Auth / inter-module
  clients are opt-in (see above).
