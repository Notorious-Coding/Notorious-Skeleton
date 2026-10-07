using NotoriousSkeleton.Catalog.Persistence.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;

namespace NotoriousSkeleton.Catalog.Persistence.EntityFramework;

internal class CatalogDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; }

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.Entity<ProductEntity>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
        });
    }
}
