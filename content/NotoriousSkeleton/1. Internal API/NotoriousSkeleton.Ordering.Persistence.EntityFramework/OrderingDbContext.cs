using NotoriousSkeleton.Ordering.Persistence.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;

namespace NotoriousSkeleton.Ordering.Persistence.EntityFramework;

internal class OrderingDbContext : DbContext
{
    public DbSet<OrderEntity> Orders { get; set; }

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("ordering");
        modelBuilder.Entity<OrderEntity>(e =>
        {
            e.Property(o => o.UnitPrice).HasPrecision(18, 2);
        });
    }
}
