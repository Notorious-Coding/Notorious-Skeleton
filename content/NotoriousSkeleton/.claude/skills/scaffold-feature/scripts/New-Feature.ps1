<#
.SYNOPSIS
    Scaffolds a new feature (Query or Command) in a NotoriousSkeleton module
    following the project's Clean/Onion + CQRS-style conventions.

.DESCRIPTION
    Generates the boilerplate files for one use case:

    Query   -> QueryFeature, I<UseCase>ReadonlyRepository, <Model>ReadModel,
               EF<UseCase>ReadonlyRepository (Persistence layer).
    Command -> CommandFeature, <UseCase>CommandParameters, <UseCase>Errors,
               and a unit-test skeleton.

    Application layout (same for every module):
      Queries/<UseCase>/{Providers,ReadModels}/
      Commands/<UseCase>/
      Providers/      (domain repositories / providers used by commands, IUserProvider)
      Common/         (AuthenticatedFeature)

    It ONLY creates the new files. The wiring that needs judgment (DI
    registrations + the controller action) is left to the caller / SKILL.md,
    because those are edits to existing files whose exact shape varies.

    Every generated file compiles as a skeleton and marks the domain-specific
    parts with `// TODO:` so you fill in fields and logic, not plumbing.

.PARAMETER Module
    The module (bounded context) the feature belongs to, PascalCase. E.g.
    "Catalog", "Ordering". Maps to NotoriousSkeleton.<Module>.* projects.

.PARAMETER Area
    The bounded concept, PascalCase singular. E.g. "Product", "Order".
    Maps to Persistence/Repository/<Area>/... and to <Area>Controller.

.PARAMETER UseCase
    The use case name, PascalCase. E.g. "GetAllProducts", "PlaceOrder".
    Becomes the feature class name prefix and the readonly-repo method name.

.PARAMETER Kind
    "Query" or "Command".

.PARAMETER ReadModel
    (Query only) The read model base name, PascalCase singular. E.g. "Product"
    -> ProductReadModel. Defaults to <Area>.

.PARAMETER UserScoped
    (Query only) If set, the query is scoped to the current user (inherits
    AuthenticatedFeature, repo takes a userId, like GetOrderById). If omitted,
    generates a plain query (like GetProductsByIds).

.PARAMETER RepoRoot
    Repo root. Auto-detected by walking up to the folder containing NotoriousSkeleton.slnx.

.EXAMPLE
    ./New-Feature.ps1 -Module Ordering -Area Order -UseCase GetAllUserOrders -Kind Query -ReadModel Order -UserScoped

.EXAMPLE
    ./New-Feature.ps1 -Module Catalog -Area Product -UseCase RenameProduct -Kind Command
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Module,
    [Parameter(Mandatory)][string]$Area,
    [Parameter(Mandatory)][string]$UseCase,
    [Parameter(Mandatory)][ValidateSet('Query', 'Command')][string]$Kind,
    [string]$ReadModel,
    [switch]$UserScoped,
    [string]$RepoRoot
)

$ErrorActionPreference = 'Stop'

function Find-RepoRoot {
    param([string]$Start)
    $dir = Get-Item -LiteralPath $Start
    while ($null -ne $dir) {
        if (Test-Path -LiteralPath (Join-Path $dir.FullName 'NotoriousSkeleton.slnx')) { return $dir.FullName }
        $dir = $dir.Parent
    }
    throw "Could not locate repo root (no NotoriousSkeleton.slnx found above '$Start'). Pass -RepoRoot explicitly."
}

if (-not $RepoRoot) { $RepoRoot = Find-RepoRoot -Start $PSScriptRoot }
if (-not $ReadModel) { $ReadModel = $Area }

$P = "NotoriousSkeleton.$Module"  # project-name prefix
$apiRoot = Join-Path $RepoRoot '1. Internal API'
$appRoot = Join-Path $apiRoot "$P.Application"
$persistenceRoot = Join-Path $apiRoot "$P.Persistence.EntityFramework"
$testsRoot = Join-Path $apiRoot "$P.Unit.Tests"

if (-not (Test-Path -LiteralPath $appRoot)) {
    throw "Application project not found at '$appRoot'. Wrong module name or repo root?"
}

$created = New-Object System.Collections.Generic.List[string]

function Write-File {
    param([string]$Path, [string]$Content)
    if (Test-Path -LiteralPath $Path) {
        Write-Warning "SKIP (already exists): $Path"
        return
    }
    $dir = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $enc = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllText($Path, $Content, $enc)
    $created.Add($Path) | Out-Null
    Write-Host "  created  $($Path.Substring($RepoRoot.Length).TrimStart('\','/'))"
}

if ($Kind -eq 'Query') {
    $featureNs = "$P.Application.Queries.$UseCase"
    $featureDir = Join-Path $appRoot ("Queries/$UseCase" -replace '/', [IO.Path]::DirectorySeparatorChar)

    $userIdParam = if ($UserScoped) { 'string userId' } else { '' }

    # ----- Feature -----
    if ($UserScoped) {
        $featureBody = @"
using $P.Application.Common;
using $P.Application.Providers;
using $featureNs.Providers;
using $featureNs.ReadModels;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace $featureNs;

public class ${UseCase}QueryFeature : AuthenticatedFeature
{
    private readonly I${UseCase}ReadonlyRepository _repository;
    private readonly ILogger<${UseCase}QueryFeature> _logger;

    public ${UseCase}QueryFeature(I${UseCase}ReadonlyRepository repository, IUserProvider userProvider, ILogger<${UseCase}QueryFeature> logger) : base(userProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<${ReadModel}ReadModel>> Invoke()
    {
        EnsureAuthenticated();
        _logger.LogInformation("Retrieving $UseCase (user={UserId})", User.Id);
        return await _repository.$UseCase(User.Id);
    }
}
"@
    } else {
        $featureBody = @"
using $featureNs.Providers;
using $featureNs.ReadModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace $featureNs;

public class ${UseCase}QueryFeature
{
    private readonly I${UseCase}ReadonlyRepository _repository;

    public ${UseCase}QueryFeature(I${UseCase}ReadonlyRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IEnumerable<${ReadModel}ReadModel>> Invoke() => _repository.$UseCase();
}
"@
    }
    Write-File (Join-Path $featureDir "${UseCase}QueryFeature.cs") $featureBody

    # ----- Readonly repository interface -----
    $interfaceBody = @"
using $featureNs.ReadModels;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace $featureNs.Providers;

public interface I${UseCase}ReadonlyRepository
{
    public Task<IEnumerable<${ReadModel}ReadModel>> $UseCase($userIdParam);
}
"@
    Write-File (Join-Path $featureDir ("Providers/I${UseCase}ReadonlyRepository.cs" -replace '/', [IO.Path]::DirectorySeparatorChar)) $interfaceBody

    # ----- Read model -----
    $readModelBody = @"
namespace $featureNs.ReadModels;

public class ${ReadModel}ReadModel
{
    public int Id { get; set; }

    // TODO: déclare les propriétés à afficher (types primitifs / value objects exposés en primitif).
    // C'est un DTO d'affichage : pas de logique domaine ici.
}
"@
    Write-File (Join-Path $featureDir ("ReadModels/${ReadModel}ReadModel.cs" -replace '/', [IO.Path]::DirectorySeparatorChar)) $readModelBody

    # ----- EF readonly repository implementation -----
    $whereClause = if ($UserScoped) { ".Where(e => e.UserId == userId)`r`n            " } else { '' }
    $efBody = @"
using $featureNs.Providers;
using $featureNs.ReadModels;
using $P.Persistence.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;

namespace $P.Persistence.EntityFramework.Repository.$Area;

internal class EF${UseCase}ReadonlyRepository : I${UseCase}ReadonlyRepository
{
    private readonly ${Module}DbContext _context;

    public EF${UseCase}ReadonlyRepository(${Module}DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<${ReadModel}ReadModel>> $UseCase($userIdParam)
    {
        // TODO: remplace le DbSet et la projection ci-dessous.
        // Ajoute une méthode `ToReadModel()` sur l'entité (cf. ProductEntity.ToReadModel).
        return await _context.Set<${ReadModel}Entity>().AsNoTracking()
            $whereClause.Select(e => e.ToReadModel())
            .ToListAsync();
    }
}
"@
    $efDir = Join-Path $persistenceRoot ("Repository/$Area" -replace '/', [IO.Path]::DirectorySeparatorChar)
    Write-File (Join-Path $efDir "EF${UseCase}ReadonlyRepository.cs") $efBody
}
else {
    # ===== Command =====
    $featureNs = "$P.Application.Commands.$UseCase"
    $featureDir = Join-Path $appRoot ("Commands/$UseCase" -replace '/', [IO.Path]::DirectorySeparatorChar)

    $featureBody = @"
using NotoriousSkeleton.Libraries.Result;
using $P.Application.Common;
using $P.Application.Providers;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace $featureNs;

public class ${UseCase}CommandFeature : AuthenticatedFeature
{
    // TODO: injecte le(s) repository domaine nécessaire(s) (Application/Providers), ex: I${Area}Repository _repository;
    private readonly ILogger<${UseCase}CommandFeature> _logger;

    public ${UseCase}CommandFeature(IUserProvider userProvider, ILogger<${UseCase}CommandFeature> logger) : base(userProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // TODO: choisis le type de retour : Result<TReadModel> (renvoie une vue) ou Result (succès/échec seul).
    public async Task<Result> Invoke(${UseCase}CommandParameters @params)
    {
        EnsureAuthenticated();
        string userId = User.Id;

        // 1) Valide les paramètres -> List<Error> (${UseCase}Errors.Xxx)
        //    if (errors.Count > 0) { _logger.LogDebug("$UseCase rejected: {@Errors}", errors); return errors; }
        // 2) Validations complémentaires via provider/repository (existence, unicité) -> ${UseCase}Errors.Xxx
        // 3) Construis l'agrégat (son constructeur valide et lève DomainException en dernier recours)
        //    puis await _repository.Save(aggregate);
        // 4) return <ReadModel>.FromDomain(...);  (ou Result.Success())
        _logger.LogInformation("$UseCase (user={UserId})", userId);
        throw new NotImplementedException("TODO: implémente $UseCase");
    }
}
"@
    Write-File (Join-Path $featureDir "${UseCase}CommandFeature.cs") $featureBody

    $paramsBody = @"
namespace $featureNs;

public class ${UseCase}CommandParameters
{
    // TODO: déclare les propriétés d'entrée (getters only) et le constructeur, cf. AddProductCommandParameters.
}
"@
    Write-File (Join-Path $featureDir "${UseCase}CommandParameters.cs") $paramsBody

    $errorsBody = @"
using NotoriousSkeleton.Libraries.Result;

namespace $featureNs;

public static class ${UseCase}Errors
{
    // TODO: déclare les erreurs métier de ce use case, cf. AddProductErrors.
    // public static Error SomethingInvalid => new Error("SOMETHING_INVALID", "Explicit message.");
}
"@
    Write-File (Join-Path $featureDir "${UseCase}Errors.cs") $errorsBody

    # ----- Unit test skeleton -----
    $testBody = @"
using FakeItEasy;

using $P.Application.Authentication;
using $P.Application.Providers;
using $featureNs;

using Microsoft.Extensions.Logging;

namespace $P.Unit.Tests.Features;

public class ${UseCase}CommandFeatureUnitTests
{
    private readonly ${UseCase}CommandFeature _testedFeature;
    private readonly IUserProvider _userProvider;
    private readonly ILogger<${UseCase}CommandFeature> _logger;

    public ${UseCase}CommandFeatureUnitTests()
    {
        _userProvider = A.Fake<IUserProvider>();
        _logger = A.Fake<ILogger<${UseCase}CommandFeature>>();
        // TODO: fake les repositories injectés puis passe-les au constructeur.
        _testedFeature = new ${UseCase}CommandFeature(_userProvider, _logger);

        A.CallTo(() => _userProvider.IsAuthenticated).Returns(true);
        A.CallTo(() => _userProvider.User).Returns(new User("toto"));
    }

    // TODO: un [Fact] par règle métier (cf. AddProductCommandFeatureUnitTests).
}
"@
    Write-File (Join-Path $testsRoot ("Features/${UseCase}CommandFeatureUnitTests.cs" -replace '/', [IO.Path]::DirectorySeparatorChar)) $testBody
}

Write-Host ""
Write-Host "Done. $($created.Count) file(s) created for $Kind '$UseCase' in module '$Module' (area '$Area')." -ForegroundColor Green
Write-Host ""
Write-Host "NEXT — wire it up (edits to existing files, see SKILL.md):" -ForegroundColor Cyan
$featureSuffix = if ($Kind -eq 'Query') { 'QueryFeature' } else { 'CommandFeature' }
Write-Host "  1. Register the feature in Add${Module}Features():"
Write-Host "     1. Internal API/$P.Application.DependencyInjection.Microsoft/ServiceCollectionExtensions.cs"
Write-Host "        .AddScoped<${UseCase}${featureSuffix}>()"
if ($Kind -eq 'Query') {
    Write-Host "  2. Register the readonly repo in Add${Module}Persistence():"
    Write-Host "     1. Internal API/$P.Persistence.EntityFramework.Extensions/ServiceCollectionExtensions.cs"
    Write-Host "        .AddScoped<I${UseCase}ReadonlyRepository, EF${UseCase}ReadonlyRepository>()"
    Write-Host "  3. Add the ToReadModel() projection on the EF entity."
    Write-Host "  4. Add the controller action (GET) on ${Area}Controller."
}
else {
    Write-Host "  2. Add the controller action (POST) on ${Area}Controller."
}
Write-Host "  Finally: dotnet build NotoriousSkeleton.slnx"
