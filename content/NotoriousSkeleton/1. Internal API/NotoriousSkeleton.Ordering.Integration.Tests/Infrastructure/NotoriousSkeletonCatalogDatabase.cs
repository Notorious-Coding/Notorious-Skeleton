using NotoriousSkeleton.Catalog.Persistence.EntityFramework;

using Microsoft.EntityFrameworkCore;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.SqlServer;

namespace NotoriousSkeleton.Ordering.Integration.Tests.Infrastructure;

public class NotoriousSkeletonCatalogDatabase : SqlServerContainerInfrastructure
{
    internal CatalogDbContext DbContext => GetDbContext();

    public NotoriousSkeletonCatalogDatabase(EnvironmentId environmentId, ITestLogger logger, IRegistry registry) : base(environmentId, logger, registry)
    {
        DbPrefix = "NotoriousSkeleton_Catalog";
        TableToIgnore = ["__EFMigrationsHistory"];
        SchemasToInclude = ["catalog"];
    }

    protected override string ConnectionStringKey => "ConnectionStrings:Catalog";

    public override async Task Initialize()
    {
        await base.Initialize();

        await using CatalogDbContext dbContext = GetDbContext();
        await dbContext.Database.MigrateAsync();
    }

    private CatalogDbContext GetDbContext()
    {
        DbContextOptions<CatalogDbContext> options = new DbContextOptionsBuilder<CatalogDbContext>().UseSqlServer(GetDatabaseConnectionString()).Options;
        return new CatalogDbContext(options);
    }
}
