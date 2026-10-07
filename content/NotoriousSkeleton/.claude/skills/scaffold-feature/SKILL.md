---
name: scaffold-feature
description: >-
  Scaffold a new use case (Query or Command Feature) in a NotoriousSkeleton .NET
  backend module (Catalog, Ordering, …) following its Clean/Onion + CQRS-style
  conventions. Use this whenever the user wants to add a backend feature, use case,
  endpoint, query, command, "GetAll…", "Add…", "Place…", a new readonly repository /
  ReadModel, or a new controller action in the "1. Internal API" projects — even if
  they don't say the word "scaffold". Generates the feature class, its
  providers/read model (or parameters/errors), the EF readonly repository, a
  unit-test skeleton, and lists the DI + controller wiring. Prefer this over
  hand-writing the files so the layering, namespaces, and registrations stay
  consistent.
---

# Scaffold a module feature

This repo is a modular monolith with a strict Clean/Onion layering and a
CQRS-style split: **reads** go Feature → `I<UseCase>ReadonlyRepository` →
`ReadModel` (projected straight from EF, no domain), **writes** go Feature →
Domain aggregate → repository (`Application/Providers`). Every use case is a
**Feature class** with a single `Invoke(...)`, registered in DI, and injected into
a controller.

Getting all of that consistent by hand is tedious and easy to get subtly wrong
(namespaces, the `internal` EF class, two separate DI registrations). This skill
generates the skeleton deterministically, then you fill in the domain-specific
parts and do the few contextual edits.

Read the conventions in the repo `CLAUDE.md` if you need the full rationale —
this skill assumes them.

## Step 1 — Gather the inputs

Ask the user (or infer from their request) only what you actually need:

- **Module**: the bounded context — `Catalog`, `Ordering`, or any module created
  with `scaffold-module`.
- **Kind**: `Query` (a read/display) or `Command` (a write/state change).
- **Area**: the bounded concept, PascalCase singular — e.g. `Product`, `Order`.
  Used for the EF repository folder (`Repository/<Area>/`) and the controller
  (`<Area>Controller`).
- **UseCase**: PascalCase — e.g. `GetAllUserOrders`, `RenameProduct`.
  This becomes the feature class prefix and the readonly-repo method name.
- **Query only** — **ReadModel** base name (defaults to `Area`) and whether the
  read is **user-scoped** (filtered by the current user, like `GetOrderById`) or
  shared by everyone (like `GetProductsByIds`).

If the user is vague, pick sensible defaults and say what you chose — don't stall
on a form.

## Step 2 — Run the generator

The script creates only the new files (safe, idempotent — it skips files that
already exist). Run it from the repo root:

```powershell
& ".claude/skills/scaffold-feature/scripts/New-Feature.ps1" -Module <Module> -Area <Area> -UseCase <UseCase> -Kind <Query|Command> [-ReadModel <Name>] [-UserScoped]
```

Examples:

```powershell
# User-scoped query
& ".claude/skills/scaffold-feature/scripts/New-Feature.ps1" -Module Ordering -Area Order -UseCase GetAllUserOrders -Kind Query -ReadModel Order -UserScoped

# Shared query (no user filter)
& ".claude/skills/scaffold-feature/scripts/New-Feature.ps1" -Module Catalog -Area Product -UseCase GetFeaturedProducts -Kind Query

# Command
& ".claude/skills/scaffold-feature/scripts/New-Feature.ps1" -Module Catalog -Area Product -UseCase RenameProduct -Kind Command
```

Generated layout (in `NotoriousSkeleton.<Module>.Application`):

```
Queries/<UseCase>/<UseCase>QueryFeature.cs
Queries/<UseCase>/Providers/I<UseCase>ReadonlyRepository.cs
Queries/<UseCase>/ReadModels/<ReadModel>ReadModel.cs
Commands/<UseCase>/<UseCase>CommandFeature.cs
Commands/<UseCase>/<UseCase>CommandParameters.cs
Commands/<UseCase>/<UseCase>Errors.cs
```

plus `Repository/<Area>/EF<UseCase>ReadonlyRepository.cs` (Persistence) for a query
and `Features/<UseCase>CommandFeatureUnitTests.cs` (Unit.Tests) for a command.

## Step 3 — Wire it up (edits to existing files)

These are contextual edits the script deliberately leaves to you. Make them with
the Edit tool, matching the surrounding style.

**Always — register the Feature** in `Add<Module>Features()`, adding the `using`
for the feature namespace:
`1. Internal API/NotoriousSkeleton.<Module>.Application.DependencyInjection.Microsoft/ServiceCollectionExtensions.cs`

```csharp
.AddScoped<GetAllUserOrdersQueryFeature>()   // or <RenameProductCommandFeature>()
```

**Query only — register the readonly repository** (interface → the `internal` EF
impl) in `Add<Module>Persistence()`, adding both `using`s:
`1. Internal API/NotoriousSkeleton.<Module>.Persistence.EntityFramework.Extensions/ServiceCollectionExtensions.cs`

```csharp
.AddScoped<IGetAllUserOrdersReadonlyRepository, EFGetAllUserOrdersReadonlyRepository>()
```

**Command — register its repository / provider** (if new) in the same
`Add<Module>Persistence()` (or the client adapter's `.Extensions` for a provider
reading another module).

**Add the controller action.** Reuse the area's controller if it exists
(`<Area>Controller` in `1. Internal API/NotoriousSkeleton.<Module>/Controllers/`);
otherwise create one modeled on `ProductController` (`[ApiController]`,
`[ApiVersion(1.0)]`, `[Authorize]`, route `api/v{version:apiVersion}/<area-lowercase>`,
constructor injecting the Feature). Add the method:

```csharp
// Query
[HttpGet]
public async Task<ActionResult<IEnumerable<OrderReadModel>>> GetAll()
    => Ok(await _getAllUserOrdersQueryFeature.Invoke());

// Command
[HttpPost]
public async Task<ActionResult<Result<ProductReadModel>>> Rename(RenameProductCommandParameters @params)
    => StatusCode((int)HttpStatusCode.Created, await _renameProductCommandFeature.Invoke(@params));
```

## Step 4 — Fill in the domain-specific parts (the `// TODO:`s)

The skeletons throw / return nothing until you complete them:

- **Query**: add the display properties on `<Model>ReadModel`; in the EF repo,
  point at the right `DbSet` and add a projection on the EF entity returning the
  new ReadModel (mirror `ProductEntity.ToReadModel()`). The generated EF repo calls
  `e.ToReadModel()` on `<ReadModel>Entity`: it does not compile until that
  projection returns the new ReadModel type.
- **Command**: define `<UseCase>CommandParameters` (getter-only props +
  constructor, cf. `AddProductCommandParameters`); validate the parameters and
  return `<UseCase>Errors.Xxx` (`return errors;`); add provider/repository
  validations (existence…); build the aggregate through its validating constructor
  (it throws `DomainException` as a last-resort guard); `Save`; return the read
  model or `Result.Success()`. Then flesh out the unit-test `[Fact]`s.

Log messages are written in **English** — keep that in the messages.

## Step 5 — Build & test

```bash
dotnet build NotoriousSkeleton.slnx
```

Fix any compile errors (usually a missing `using` on a DI registration or a
`DbSet`/projection name in the EF repo), then run the relevant tests:

```bash
dotnet test --project "1. Internal API/NotoriousSkeleton.<Module>.Unit.Tests" --filter-method "*<UseCase>*"
```

## Scope & non-goals

- This scaffolds the **Application + Persistence + unit test** files. It does
  **not** create Domain aggregates, EF entities, `DbSet`s, or migrations — a
  command over a brand-new aggregate needs those authored first (Domain aggregate
  with a validating constructor, entity + `IMappable`, `DbContext` config, then
  `5. Scripts/Add-ModuleMigration.ps1 <Name> -Module <Module>`). Tell the user when
  that groundwork is missing rather than generating something that can't compile.
