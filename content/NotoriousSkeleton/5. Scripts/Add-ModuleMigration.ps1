<#
.SYNOPSIS
  Génère une migration EF Core pour un module donné du monolithe modulaire NotoriousSkeleton.

.DESCRIPTION
  Chaque module (bounded context) possède son propre DbContext et son projet de
  persistance. Comme plusieurs DbContext coexistent sous le même projet de démarrage
  (NotoriousSkeleton.API), EF exige de préciser --context : ce script le fait automatiquement à
  partir du module choisi.

  Pour ajouter un nouveau module, ajoute simplement une entrée dans $Modules.

.PARAMETER MigrationName
  Nom de la migration (ex: AddProductTable).

.PARAMETER Module
  Module ciblé : Catalog ou Ordering (ou All pour générer dans tous les modules).

.EXAMPLE
  ./Add-ModuleMigration.ps1 AddProductTable -Module Catalog

.EXAMPLE
  ./Add-ModuleMigration.ps1 InitOrdering -Module Ordering
#>
param(
  [Parameter(Mandatory = $true, Position = 0)]
  [ValidateNotNullOrEmpty()]
  [string]$MigrationName,

  [Parameter(Mandatory = $true, Position = 1)]
  [ValidateNotNullOrEmpty()]
  [string]$Module,

  # Projet de démarrage (host ASP.NET Core). Relatif à la racine du repo.
  [Parameter(Position = 2)]
  [string]$StartupRelPath = '1. Internal API/NotoriousSkeleton.API'
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
  # Sinon on renvoie tel quel : dotnet remontera l'erreur si le chemin est faux.
  return $candidate
}

function Add-MigrationForModule([string]$moduleName) {
  $config      = $Modules[$moduleName]
  $startupPath = Get-FullPath $StartupRelPath
  $projectPath = Get-FullPath $config.ProjectRelPath

  $efArgs = @(
    'ef', 'migrations', 'add', $MigrationName,
    '--startup-project', $startupPath,
    '--project', $projectPath,
    '--context', $config.Context
  )

  Write-Host ""
  Write-Host "==> [$moduleName] dotnet $($efArgs -join ' ')" -ForegroundColor Cyan

  & dotnet @efArgs
  if ($LASTEXITCODE -ne 0) {
    throw "La migration a échoué pour le module '$moduleName' (code $LASTEXITCODE)."
  }
  Write-Host "OK migration '$MigrationName' générée pour le module '$moduleName'." -ForegroundColor Green
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
  Add-MigrationForModule $target
}

exit 0
