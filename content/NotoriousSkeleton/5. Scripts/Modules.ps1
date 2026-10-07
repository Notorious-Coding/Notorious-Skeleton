# Source unique des modules (bounded contexts) du monolithe, pour les scripts de
# migration EF Core (Add-ModuleMigration, Create-SQLMigrationsFile, Update-Database).
#
# Chaque entrée mappe un module vers son projet de persistance et son DbContext.
# Ajoute une entrée quand un nouveau module est créé — la skill scaffold-module
# le fait automatiquement.
[ordered]@{
  Catalog  = @{
    ProjectRelPath = '1. Internal API/NotoriousSkeleton.Catalog.Persistence.EntityFramework'
    Context        = 'CatalogDbContext'
  }
  Ordering = @{
    ProjectRelPath = '1. Internal API/NotoriousSkeleton.Ordering.Persistence.EntityFramework'
    Context        = 'OrderingDbContext'
  }
}
