<#
.SYNOPSIS
  Applique les migrations EF Core à la base pour un module donné du monolithe
  modulaire NotoriousSkeleton.

.DESCRIPTION
  Chaque module possède son propre DbContext. Comme plusieurs DbContext coexistent
  sous le même projet de démarrage (NotoriousSkeleton.API), EF exige --context : ce script le
  déduit du module choisi.

  Pour ajouter un module, ajoute une entrée dans $Modules.

.PARAMETER Module
  Module ciblé : Catalog ou Ordering (ou All pour tous les modules).

.PARAMETER Migration
  Optionnel : cible une migration précise (ex: InitialCreate). Vide => dernière migration.

.EXAMPLE
  ./Update-Database.ps1 -Module Catalog

.EXAMPLE
  ./Update-Database.ps1 -Module All
#>
param(
  [Parameter(Mandatory = $true, Position = 0)]
  [ValidateNotNullOrEmpty()]
  [string]$Module,

  # Optionnel : cible la migration (ex: "InitialCreate"). Si vide => dernière migration.
  # Ignoré quand -Module All (n'a pas de sens sur plusieurs contextes).
  [Parameter(Position = 1)]
  [string]$Migration,

  # Projet de démarrage (host ASP.NET Core). Relatif à la racine du repo.
  [Parameter(Position = 2)]
  [string]$StartupRelPath = '1. Internal API/NotoriousSkeleton.API',

  # Optionnel : connexion override.
  [string]$Connection = "Server=localhost,1433;Database=notoriousskeleton;User Id=sa;Password=Skeleton@Passw0rd;TrustServerCertificate=True",

  # Optionnel : skip build.
  [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'

# Racine du repo = dossier parent de "5. Scripts/"
$RepoRoot = Split-Path -Parent $PSScriptRoot

# Table des modules (source unique partagée par les scripts de migration).
$Modules = & (Join-Path $PSScriptRoot 'Modules.ps1')

# Validation du module choisi (remplace un ValidateSet figé : reste synchro avec Modules.ps1).
$knownModules = @($Modules.Keys) + 'All'
if ($Module -notin $knownModules) {
  Write-Error "Module inconnu '$Module'. Valeurs possibles : $($knownModules -join ', ')."
  exit 1
}

function Get-FullPath([string]$relPath) {
  $candidate = Join-Path -Path $RepoRoot -ChildPath $relPath
  if (Test-Path -LiteralPath $candidate) {
    return (Resolve-Path -LiteralPath $candidate).Path
  }
  return $candidate
}

function Update-DatabaseForModule([string]$moduleName) {
  $config      = $Modules[$moduleName]
  $startupPath = Get-FullPath $StartupRelPath
  $projectPath = Get-FullPath $config.ProjectRelPath

  $efArgs = @('ef', 'database', 'update')
  # La cible de migration n'a de sens que pour un module unique.
  if ($Migration -and $Module -ne 'All') { $efArgs += $Migration }
  $efArgs += @(
    '--startup-project', $startupPath,
    '--project', $projectPath,
    '--context', $config.Context
  )
  if ($Connection) { $efArgs += @('--connection', $Connection) }
  if ($NoBuild)    { $efArgs += '--no-build' }

  Write-Host ""
  Write-Host "==> [$moduleName] dotnet $($efArgs -join ' ')" -ForegroundColor Cyan
  & dotnet @efArgs
  if ($LASTEXITCODE -ne 0) {
    throw "La mise à jour de la base a échoué pour le module '$moduleName' (code $LASTEXITCODE)."
  }
  Write-Host "OK base à jour pour le module '$moduleName'." -ForegroundColor Green
}

# Vérif rapide du SDK .NET
try {
  & dotnet --version | Out-Null
} catch {
  Write-Error "Le SDK .NET n'est pas disponible dans le PATH. Installe-le puis réessaie."
  exit 1
}

$targets = if ($Module -eq 'All') { @($Modules.Keys) } else { @($Module) }

foreach ($target in $targets) {
  Update-DatabaseForModule $target
}

exit 0
