#Requires -Version 7
<#
.SYNOPSIS
    Scaffolds a brand-new backend module in the NotoriousSkeleton modular monolith, mirroring
    the Catalog module's Clean/Onion project layout.

.DESCRIPTION
    Creates the 8 core projects that make up a module (Domain, Application,
    Application.DependencyInjection.Microsoft, Persistence.EntityFramework,
    Persistence.EntityFramework.Extensions, the <Module> API-surface project,
    Unit.Tests and Integration.Tests) under "1. Internal API/", each with the
    right csproj references, InternalsVisibleTo, and a compiling skeleton.

    It only ever CREATES files — it never overwrites an existing one, so it is
    safe to re-run. All files are written UTF-8 with BOM to match the existing
    sources. Optionally adds the new projects to NotoriousSkeleton.slnx by writing the folder
    structure into the XML directly (numbered sub-folders that mirror Catalog:
    <n>.1 API, <n>.2 Business, <n>.3 Infrastructures/Persistence, <n>.4
    Integration Tests, <n>.5 Unit Tests).

.PARAMETER Module
    PascalCase, singular bounded-context name, e.g. Billing, Coaching, Messaging.
    Used verbatim as the "<Module>" segment in every project name and namespace.

.PARAMETER Schema
    SQL Server schema the module's DbContext uses (HasDefaultSchema). Defaults to
    the lower-cased module name.

.PARAMETER SolutionFolder
    Virtual solution-folder label under "1. Internal API/" where the projects are
    placed in NotoriousSkeleton.slnx. Defaults to the module name; pass e.g. "1.4 Billing" to
    match the numbered convention. When the label starts with a number, that
    number is reused as the prefix for the numbered sub-folders (e.g. "1.4
    Billing" -> "1.4.1 API", "1.4.2 Business", …).

.PARAMETER AddToSolution
    When set (default), runs `dotnet sln add` for each project. Pass
    -AddToSolution:$false to wire NotoriousSkeleton.slnx by hand instead.

.EXAMPLE
    & ".claude/skills/scaffold-module/scripts/New-Module.ps1" -Module Billing -SolutionFolder "1.4 Billing"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Z][A-Za-z0-9]*$', ErrorMessage = "Module must be PascalCase with no spaces, e.g. 'Billing'.")]
    [string]$Module,

    [string]$Schema,

    [string]$SolutionFolder,

    [bool]$AddToSolution = $true
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Schema)) { $Schema = $Module.ToLowerInvariant() }
if ([string]::IsNullOrWhiteSpace($SolutionFolder)) { $SolutionFolder = $Module }

# Repo root = four levels above scripts/ (scaffold-module/skills/.claude/<root>)
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
$ApiRoot  = Join-Path $RepoRoot '1. Internal API'
$Slnx     = Join-Path $RepoRoot 'NotoriousSkeleton.slnx'

if (-not (Test-Path $ApiRoot)) {
    throw "Could not locate '1. Internal API' under repo root '$RepoRoot'. Run from the NotoriousSkeleton repo."
}

$Utf8Bom = New-Object System.Text.UTF8Encoding($true)
$script:Created = New-Object System.Collections.Generic.List[string]
$script:Skipped = New-Object System.Collections.Generic.List[string]

function Add-ModuleFile {
    param([string]$RelPath, [string]$Content)
    $full = Join-Path $ApiRoot $RelPath
    $dir  = Split-Path $full -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    if (Test-Path $full) {
        $script:Skipped.Add($RelPath)
        return
    }
    # Normalise to CRLF to match the existing sources.
    $normalized = ($Content -replace "`r`n", "`n") -replace "`n", "`r`n"
    [System.IO.File]::WriteAllText($full, $normalized, $Utf8Bom)
    $script:Created.Add($RelPath)
}

$P = "NotoriousSkeleton.$Module"  # project-name prefix

# ---------------------------------------------------------------------------
# 1. Domain
# ---------------------------------------------------------------------------
Add-ModuleFile "$P.Domain\$P.Domain.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
	</PropertyGroup>
	<ItemGroup>
		<ProjectReference Include="..\..\3. Libraries\NotoriousSkeleton.Libraries.Domain.Tools\NotoriousSkeleton.Libraries.Domain.Tools.csproj" />
		<ProjectReference Include="..\..\3. Libraries\NotoriousSkeleton.Libraries.Result\NotoriousSkeleton.Libraries.Result.csproj" />
	</ItemGroup>
	<ItemGroup>
		<InternalsVisibleTo Include="$P.Integration.Tests" />
		<InternalsVisibleTo Include="$P.Persistence.EntityFramework" />
	</ItemGroup>
</Project>
"@

# ---------------------------------------------------------------------------
# 2. Application
# ---------------------------------------------------------------------------
Add-ModuleFile "$P.Application\$P.Application.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
	</ItemGroup>
	<ItemGroup>
	  <ProjectReference Include="..\..\3. Libraries\NotoriousSkeleton.Libraries.Enumeration\NotoriousSkeleton.Libraries.Enumeration.csproj" />
	  <ProjectReference Include="..\$P.Domain\$P.Domain.csproj" />
	</ItemGroup>
</Project>
"@

# ---------------------------------------------------------------------------
# 3. Application.DependencyInjection.Microsoft
# ---------------------------------------------------------------------------
Add-ModuleFile "$P.Application.DependencyInjection.Microsoft\$P.Application.DependencyInjection.Microsoft.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\$P.Application\$P.Application.csproj" />
  </ItemGroup>

</Project>
"@

Add-ModuleFile "$P.Application.DependencyInjection.Microsoft\ServiceCollectionExtensions.cs" @"
using Microsoft.Extensions.DependencyInjection;

namespace $P.Application.DependencyInjection.Microsoft;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add${Module}Features(this IServiceCollection services)
    {
        // Register each Feature (use case) here as the module grows, e.g.:
        //   .AddScoped<GetAll${Module}sQueryFeature>()
        //   .AddScoped<Add${Module}CommandFeature>()
        return services;
    }
}
"@

# ---------------------------------------------------------------------------
# 4. Persistence.EntityFramework
# ---------------------------------------------------------------------------
Add-ModuleFile "$P.Persistence.EntityFramework\$P.Persistence.EntityFramework.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
	</PropertyGroup>
	<ItemGroup>
		<InternalsVisibleTo Include="$P.Persistence.EntityFramework.Extensions" />
		<InternalsVisibleTo Include="$P.Integration.Tests" />
	</ItemGroup>
	<ItemGroup>
	  <PackageReference Include="Microsoft.EntityFrameworkCore" />
	  <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
	    <PrivateAssets>all</PrivateAssets>
	    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	  </PackageReference>
	  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
	</ItemGroup>
	<ItemGroup>
	  <ProjectReference Include="..\$P.Application\$P.Application.csproj" />
	  <ProjectReference Include="..\$P.Domain\$P.Domain.csproj" />
	</ItemGroup>
</Project>
"@

Add-ModuleFile "$P.Persistence.EntityFramework\${Module}DbContext.cs" @"
using Microsoft.EntityFrameworkCore;

namespace $P.Persistence.EntityFramework;

internal class ${Module}DbContext : DbContext
{
    public ${Module}DbContext(DbContextOptions<${Module}DbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("$Schema");

        // Add DbSet<...> properties above and entity/seed configuration here as
        // the module gains aggregates.
    }
}
"@

# ---------------------------------------------------------------------------
# 5. Persistence.EntityFramework.Extensions
# ---------------------------------------------------------------------------
Add-ModuleFile "$P.Persistence.EntityFramework.Extensions\$P.Persistence.EntityFramework.Extensions.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\$P.Domain\$P.Domain.csproj" />
		<ProjectReference Include="..\$P.Persistence.EntityFramework\$P.Persistence.EntityFramework.csproj" />
	</ItemGroup>
</Project>
"@

Add-ModuleFile "$P.Persistence.EntityFramework.Extensions\ServiceCollectionExtensions.cs" @"
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace $P.Persistence.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add${Module}Persistence(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            // Register the module's readonly repositories (queries) and domain
            // repositories (commands) here as they are added, e.g.:
            //   .AddScoped<IGetAll${Module}sReadonlyRepository, Ef${Module}ReadonlyRepository>()
            //   .AddScoped<I${Module}Repository, Ef${Module}Repository>()
            .AddDbContext<${Module}DbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("$Module"),
                    sqlServer => sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", "$Schema"));
            });
    }
}
"@

# ---------------------------------------------------------------------------
# 6. Module API surface (NotoriousSkeleton.<Module>)
# ---------------------------------------------------------------------------
Add-ModuleFile "$P\$P.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
	</PropertyGroup>
	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
	</ItemGroup>
	<ItemGroup>
	  <ProjectReference Include="..\$P.Application.DependencyInjection.Microsoft\$P.Application.DependencyInjection.Microsoft.csproj" />
	  <ProjectReference Include="..\$P.Application\$P.Application.csproj" />
	  <ProjectReference Include="..\$P.Persistence.EntityFramework.Extensions\$P.Persistence.EntityFramework.Extensions.csproj" />
	</ItemGroup>
	<ItemGroup>
		<PackageReference Include="Asp.Versioning.Mvc" />
	</ItemGroup>
</Project>
"@

Add-ModuleFile "$P\AssemblyMarker.cs" @"
using System.Reflection;

namespace $P;

public static class AssemblyMarker
{
    public static Assembly CurrentAssembly => typeof(AssemblyMarker).Assembly;
}
"@

Add-ModuleFile "$P\ModuleExtensions.cs" @"
using $P.Application.DependencyInjection.Microsoft;
using $P.Persistence.EntityFramework.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace $P;

public static class ModuleExtensions
{
    public static IServiceCollection Add${Module}Module(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Add${Module}Features()
                       .Add${Module}Persistence(configuration);
    }

    public static IMvcBuilder Add${Module}Controllers(this IMvcBuilder builder)
    {
        return builder.AddApplicationPart(AssemblyMarker.CurrentAssembly);
    }
}
"@

# Keep the Controllers folder in the tree from the start.
Add-ModuleFile "$P\Controllers\.gitkeep" ""

# ---------------------------------------------------------------------------
# 7. Unit tests
# ---------------------------------------------------------------------------
Add-ModuleFile "$P.Unit.Tests\$P.Unit.Tests.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		<IsPackable>false</IsPackable>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="coverlet.collector">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.Extensions.TimeProvider.Testing" />
		<PackageReference Include="Microsoft.NET.Test.Sdk" />
		<PackageReference Include="FakeItEasy" />
		<PackageReference Include="xunit.v3" />
		<PackageReference Include="xunit.runner.visualstudio">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>
	</ItemGroup>

	<ItemGroup>
	  <ProjectReference Include="..\$P.Application\$P.Application.csproj" />
	</ItemGroup>

	<ItemGroup>
		<Using Include="Xunit" />
	</ItemGroup>
</Project>
"@

# ---------------------------------------------------------------------------
# 8. Integration tests
# ---------------------------------------------------------------------------
Add-ModuleFile "$P.Integration.Tests\$P.Integration.Tests.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FakeItEasy" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
    <PackageReference Include="Microsoft.AspNetCore.SpaProxy" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" />
    <PackageReference Include="NotoriousTest" />
    <PackageReference Include="NotoriousTest.SqlServer" />
    <PackageReference Include="NotoriousTest.TestContainers" />
    <PackageReference Include="NotoriousTest.Web" />
    <PackageReference Include="NotoriousTest.XUnit" />
    <PackageReference Include="Respawn" />
    <PackageReference Include="NotoriousClient" />
    <PackageReference Include="Testcontainers.MsSql" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\NotoriousSkeleton.API\NotoriousSkeleton.API.csproj" />
    <ProjectReference Include="..\$P.Persistence.EntityFramework\$P.Persistence.EntityFramework.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
"@

# ---------------------------------------------------------------------------
# Report
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Module '$Module' scaffolded under '1. Internal API/' (schema: '$Schema')." -ForegroundColor Cyan
Write-Host ""
if ($script:Created.Count -gt 0) {
    Write-Host "Created:" -ForegroundColor Green
    $script:Created | ForEach-Object { Write-Host "  + $_" }
}
if ($script:Skipped.Count -gt 0) {
    Write-Host "Skipped (already existed):" -ForegroundColor Yellow
    $script:Skipped | ForEach-Object { Write-Host "  = $_" }
}

# ---------------------------------------------------------------------------
# Solution membership
#
# We write the folder structure straight into NotoriousSkeleton.slnx instead of shelling
# out to `dotnet sln add --solution-folder`: in the .slnx format that command
# mis-nests the projects (it collapses every non-test project into the first
# folder) and can't produce the numbered sub-folders. Building the XML here
# gives the numbered layout (e.g. 1.1.1 API, 1.1.2 Business, 1.1.3
# Infrastructures/Persistence, 1.1.4 Integration Tests, 1.1.5 Unit Tests) that
# mirrors the Catalog module exactly.
# ---------------------------------------------------------------------------

# Numeric prefix taken from the solution-folder label, e.g. "1.3 Billing" ->
# "1.1". When the label has no number, sub-folders fall back to bare names.
$numPrefix = $null
if ($SolutionFolder -match '^\s*([0-9]+(?:\.[0-9]+)*)\s') { $numPrefix = $Matches[1] }

function Get-SlnxPath([string]$relPath) { "1. Internal API/" + ($relPath -replace '\\', '/') }

# Ordered sub-folders mirroring Catalog's layout. `Sub` nests the projects one
# extra (unnumbered) level, the way Infrastructures/Persistence does.
$groups = @(
    @{ N = 1; Label = 'API'; Sub = $null; Projects = @("$P\$P.csproj") }
    @{ N = 2; Label = 'Business'; Sub = $null; Projects = @(
            "$P.Application.DependencyInjection.Microsoft\$P.Application.DependencyInjection.Microsoft.csproj",
            "$P.Application\$P.Application.csproj",
            "$P.Domain\$P.Domain.csproj") }
    @{ N = 3; Label = 'Infrastructures'; Sub = 'Persistence'; Projects = @(
            "$P.Persistence.EntityFramework.Extensions\$P.Persistence.EntityFramework.Extensions.csproj",
            "$P.Persistence.EntityFramework\$P.Persistence.EntityFramework.csproj") }
    @{ N = 4; Label = 'Integration Tests'; Sub = $null; Projects = @("$P.Integration.Tests\$P.Integration.Tests.csproj") }
    @{ N = 5; Label = 'Unit Tests'; Sub = $null; Projects = @("$P.Unit.Tests\$P.Unit.Tests.csproj") }
)

if ($AddToSolution) {
    Write-Host ""
    Write-Host "Adding projects to NotoriousSkeleton.slnx ..." -ForegroundColor Cyan

    $slnxText = [System.IO.File]::ReadAllText($Slnx)
    $apiMarker = Get-SlnxPath "$P\$P.csproj"

    if ($slnxText -match [regex]::Escape($apiMarker)) {
        Write-Host "  = module already present in NotoriousSkeleton.slnx — left as-is." -ForegroundColor Yellow
    }
    else {
        $parentFolder = "/1. Internal API/$SolutionFolder/"
        $sb = New-Object System.Text.StringBuilder

        # Parent module folder — only if not already declared in the .slnx.
        if ($slnxText -notmatch [regex]::Escape("Name=`"$parentFolder`"")) {
            [void]$sb.AppendLine("  <Folder Name=`"$parentFolder`" />")
        }

        foreach ($g in $groups) {
            $label = if ($numPrefix) { "$numPrefix.$($g.N) $($g.Label)" } else { $g.Label }
            $topPath = "$parentFolder$label/"

            if ($g.Sub) {
                # Empty parent (e.g. Infrastructures) + a child folder holding the projects.
                [void]$sb.AppendLine("  <Folder Name=`"$topPath`" />")
                [void]$sb.AppendLine("  <Folder Name=`"$topPath$($g.Sub)/`">")
            }
            else {
                [void]$sb.AppendLine("  <Folder Name=`"$topPath`">")
            }
            foreach ($projectPath in $g.Projects) {
                [void]$sb.AppendLine("    <Project Path=`"$(Get-SlnxPath $projectPath)`" />")
            }
            [void]$sb.AppendLine("  </Folder>")
            Write-Host "  + $label"
        }

        $block = $sb.ToString().TrimEnd("`r", "`n")

        # Insert the block right before the closing </Solution> tag.
        $closeIdx = $slnxText.LastIndexOf('</Solution>')
        if ($closeIdx -lt 0) { throw "Malformed NotoriousSkeleton.slnx: no </Solution> tag found." }
        $before = $slnxText.Substring(0, $closeIdx).TrimEnd("`r", "`n")
        $after = $slnxText.Substring($closeIdx)
        $newText = "$before`r`n$block`r`n$after"
        [System.IO.File]::WriteAllText($Slnx, $newText, $Utf8Bom)
    }
}
else {
    Write-Host ""
    Write-Host "Skipped solution wiring (-AddToSolution:`$false). Add the 8 projects to NotoriousSkeleton.slnx yourself." -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# Register the module in the migration scripts' shared module list
# (5. Scripts/Modules.ps1). Add-ModuleMigration / Create-SQLMigrationsFile /
# Update-Database read this list, so the new DbContext is picked up
# automatically (with its own --context and per-schema __EFMigrationsHistory).
# ---------------------------------------------------------------------------
$modulesPs1 = Join-Path $RepoRoot '5. Scripts/Modules.ps1'
if (Test-Path $modulesPs1) {
    $modulesText = [System.IO.File]::ReadAllText($modulesPs1)
    if ($modulesText -match "(?m)^\s*$Module\s*=") {
        Write-Host ""
        Write-Host "  = '$Module' already registered in 5. Scripts/Modules.ps1 — left as-is." -ForegroundColor Yellow
    }
    else {
        $entry = @"
  $Module = @{
    ProjectRelPath = '1. Internal API/$P.Persistence.EntityFramework'
    Context        = '${Module}DbContext'
  }
"@
        $closeIdx = $modulesText.LastIndexOf('}')
        if ($closeIdx -lt 0) { throw "Malformed 5. Scripts/Modules.ps1: no closing '}' found." }
        $before  = $modulesText.Substring(0, $closeIdx).TrimEnd("`r", "`n", " ")
        $after   = $modulesText.Substring($closeIdx)
        $entry   = ($entry -replace "`r`n", "`n") -replace "`n", "`r`n"
        $newText = "$before`r`n$entry`r`n$after"
        [System.IO.File]::WriteAllText($modulesPs1, $newText, $Utf8Bom)
        Write-Host ""
        Write-Host "Registered '$Module' in 5. Scripts/Modules.ps1 (migration scripts now cover its DbContext)." -ForegroundColor Green
    }
}
else {
    Write-Host ""
    Write-Host "  ! 5. Scripts/Modules.ps1 not found — add '$Module' to the migration scripts' module list by hand." -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# Wiring checklist
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Next — manual wiring (edit existing files):" -ForegroundColor Cyan
Write-Host @"
  1. 1. Internal API/NotoriousSkeleton.API/NotoriousSkeleton.API.csproj
       Add ProjectReferences to:
         ..\$P\$P.csproj
         ..\$P.Persistence.EntityFramework.Extensions\$P.Persistence.EntityFramework.Extensions.csproj

  2. 1. Internal API/NotoriousSkeleton.API/Program.cs
       builder.Services.Add${Module}Module(builder.Configuration);   // near AddCatalogModule
       .Add${Module}Controllers()                                    // chain onto AddControllers()

  3. 1. Internal API/NotoriousSkeleton.API/appsettings.json (+ appsettings.Development.json)
       "ConnectionStrings": { "$Module": "..." }   // Development can reuse the same SQL Server host/db as Catalog

  4. Build:  dotnet build NotoriousSkeleton.slnx

  5. Migrations (once the module has its first entity + DbSet):
       Registered in 5. Scripts/Modules.ps1 automatically. Then:
         5. Scripts/Add-ModuleMigration.ps1 <Name> -Module $Module   # generate a migration
         5. Scripts/Update-Database.ps1 -Module $Module              # apply it (dev)
         5. Scripts/Create-SQLMigrationsFile.ps1                     # regenerate the combined deploy SQL
"@
