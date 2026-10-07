<#
.SYNOPSIS
  Génère UN seul script SQL idempotent regroupant les migrations de tous les
  modules (DbContext) de NotoriousSkeleton, destiné à être joué une seule fois sur le serveur
  de base de données au déploiement.

.DESCRIPTION
  Chaque module possède son propre DbContext mais tous ciblent la même base
  SQL Server et partagent la table d'historique "__EFMigrationsHistory". Le script
  idempotent de chaque contexte garde chaque migration par un test d'existence
  (IF NOT EXISTS), et la création de la table d'historique est elle-même idempotente,
  donc la concaténation des scripts est rejouable sans conflit.

  Pour ajouter un module, ajoute une entrée dans $Modules.

.PARAMETER OutputRelPath
  Chemin (relatif à la racine du repo) du fichier SQL combiné à produire.

.EXAMPLE
  ./Create-SQLMigrationsFile.ps1
#>
param(
  # Projet de démarrage (host ASP.NET Core). Relatif à la racine du repo.
  [Parameter(Position = 1)]
  [string]$StartupRelPath = '1. Internal API/NotoriousSkeleton.API',

  [Parameter(Position = 2)]
  [string]$OutputRelPath = '1. Internal API/migrations.sql'
)

$ErrorActionPreference = 'Stop'

# Racine du repo = dossier parent de "5. Scripts/"
$RepoRoot = Split-Path -Parent $PSScriptRoot

# Table des modules (source unique partagée par les scripts de migration).
$Modules = & (Join-Path $PSScriptRoot 'Modules.ps1')

function Get-FullPath([string]$relPath) {
  $candidate = Join-Path -Path $RepoRoot -ChildPath $relPath
  if (Test-Path -LiteralPath $candidate) {
    return (Resolve-Path -LiteralPath $candidate).Path
  }
  return $candidate
}

# Vérif rapide du SDK .NET
try {
  & dotnet --version | Out-Null
} catch {
  Write-Error "Le SDK .NET n'est pas disponible dans le PATH. Installe-le puis réessaie."
  exit 1
}

$startupPath = Get-FullPath $StartupRelPath
$outputPath  = Get-FullPath $OutputRelPath

# Crée le dossier cible si besoin
$outputDir = Split-Path -Parent $outputPath
if ($outputDir -and -not (Test-Path -LiteralPath $outputDir)) {
  New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$tempFiles = @()
try {
  foreach ($moduleName in $Modules.Keys) {
    $config      = $Modules[$moduleName]
    $projectPath = Get-FullPath $config.ProjectRelPath
    $tempFile    = [System.IO.Path]::GetTempFileName()
    $tempFiles  += $tempFile

    $efArgs = @(
      'ef', 'migrations', 'script',
      '--idempotent',
      '-o', $tempFile,
      '--startup-project', $startupPath,
      '--project', $projectPath,
      '--context', $config.Context
    )

    Write-Host ""
    Write-Host "==> [$moduleName] dotnet $($efArgs -join ' ')" -ForegroundColor Cyan
    & dotnet @efArgs
    if ($LASTEXITCODE -ne 0) {
      throw "Génération du script SQL échouée pour le module '$moduleName' (code $LASTEXITCODE)."
    }
  }

  # Concatène les scripts de chaque module dans un seul fichier, avec un en-tête par module.
  $builder = [System.Text.StringBuilder]::new()
  [void]$builder.AppendLine('-- =====================================================================')
  [void]$builder.AppendLine('-- Script de migration idempotent combiné (généré automatiquement).')
  [void]$builder.AppendLine('-- Ne pas éditer à la main : régénérer via 5. Scripts/Create-SQLMigrationsFile.ps1')
  [void]$builder.AppendLine('-- =====================================================================')

  $i = 0
  foreach ($moduleName in $Modules.Keys) {
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("-- ---------------------------------------------------------------------")
    [void]$builder.AppendLine("-- Module : $moduleName ($($Modules[$moduleName].Context))")
    [void]$builder.AppendLine("-- ---------------------------------------------------------------------")
    [void]$builder.AppendLine((Get-Content -LiteralPath $tempFiles[$i] -Raw))
    $i++
  }

  Set-Content -LiteralPath $outputPath -Value $builder.ToString() -Encoding UTF8

  Write-Host ""
  Write-Host "OK script idempotent combiné généré : $outputPath" -ForegroundColor Green
}
finally {
  foreach ($tempFile in $tempFiles) {
    if (Test-Path -LiteralPath $tempFile) { Remove-Item -LiteralPath $tempFile -Force }
  }
}

exit 0
