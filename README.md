# Notorious Skeleton

__A ready-to-run .NET 10 modular monolith, available as a `dotnet new` template__

[![NuGet](https://img.shields.io/nuget/v/NotoriousSkeleton)](https://www.nuget.org/packages/NotoriousSkeleton/)
[![License](https://img.shields.io/github/license/Notorious-Coding/Notorious-Skeleton)](https://github.com/Notorious-Coding/Notorious-Skeleton/blob/develop/LICENSE.txt)
[![Build Status](https://github.com/Notorious-Coding/Notorious-Skeleton/actions/workflows/prerelease.yml/badge.svg)](https://github.com/Notorious-Coding/Notorious-Skeleton/actions/workflows/prerelease.yml)

## Installation

```sh
dotnet new install NotoriousSkeleton
```

## Usage

```sh
dotnet new notoriousskeleton -n MyProduct
```

Every `NotoriousSkeleton` occurrence (projects, namespaces, folders, docker services, image names…) is replaced by
`MyProduct` (`notoriousskeleton` by `myproduct`).

Then, from the generated folder:

```sh
dotnet build MyProduct.slnx
dotnet test --project "1. Internal API/MyProduct.Catalog.Unit.Tests"
dotnet test --project "1. Internal API/MyProduct.Catalog.Integration.Tests"   # Docker required
docker compose up -d --build
```

`appsettings.Development.json` (API) and `public/configuration/configuration.development.json` (front) are not part
of the template (they are git-ignored): create them locally with your development connection strings and settings.

## What's inside

- **Backend** — ASP.NET Core (.NET 10) modular monolith, Clean/Onion layering, CQRS-style Feature classes, Result
  pattern, strongly-typed IDs and value objects. Two example modules:
  - `Catalog` — `GetAllProducts` (query), `AddProduct` (command), `GetProductsByIds` (query exposed to other modules);
  - `Ordering` — `GetOrderById` (query), `PlaceOrder` (command, reads products from `Catalog` through its externals).
- **Persistence** — EF Core + SQL Server, one `DbContext` and schema per module, migrations and PowerShell scripts
  (`5. Scripts/`).
- **Authentication** — JWT (Auth0), one user provider adapter per module.
- **Logs** — Serilog (console + rolling compact JSON files).
- **Tests** — unit tests (xUnit v3 + FakeItEasy) and integration tests with
  [NotoriousTest](https://github.com/Notorious-Coding/Notorious-Test) (SQL Server in Docker, real HTTP pipeline).
- **Front** — minimal React 19 + TypeScript + Vite app, served by the API in production.
- **Docker** — `docker-compose.yml` (API + SQL Server), development and production Dockerfiles.
- **CI/CD** — GitHub Actions: build & test, image pushed to GHCR, deployment over SSH with secrets from 1Password.
- **Claude Code** — `CLAUDE.md` and the `scaffold-module` / `scaffold-feature` skills.

## Repository layout

- `content/NotoriousSkeleton/` — the template itself (`.template.config/template.json`).
- `NotoriousSkeleton.csproj` — the template package (`dotnet pack`).
- `.github/workflows/` — pre-release (`develop`) and release (GitHub release) publication to nuget.org, after
  generating a project from the template and running its build and tests.

### Working on the template locally

```sh
dotnet pack -o out
dotnet new install ./out/NotoriousSkeleton.0.1.0.nupkg
dotnet new notoriousskeleton -n TemplateCheck -o ../TemplateCheck
dotnet new uninstall NotoriousSkeleton
```
