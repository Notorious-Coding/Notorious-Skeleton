using NotoriousSkeleton.Ordering.Persistence.EntityFramework;

using Microsoft.EntityFrameworkCore;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.SqlServer;

namespace NotoriousSkeleton.Ordering.Integration.Tests.Infrastructure;

public class NotoriousSkeletonOrderingDatabase : SqlServerContainerInfrastructure
{
    internal OrderingDbContext DbContext => GetDbContext();

    public NotoriousSkeletonOrderingDatabase(EnvironmentId environmentId, ITestLogger logger, IRegistry registry) : base(environmentId, logger, registry)
    {
        DbPrefix = "NotoriousSkeleton_Ordering";
        TableToIgnore = ["__EFMigrationsHistory"];
        SchemasToInclude = ["ordering"];
    }

    protected override string ConnectionStringKey => "ConnectionStrings:Ordering";

    public override async Task Initialize()
    {
        await base.Initialize();

        await using OrderingDbContext dbContext = GetDbContext();
        await dbContext.Database.MigrateAsync();
    }

    private OrderingDbContext GetDbContext()
    {
        DbContextOptions<OrderingDbContext> options = new DbContextOptionsBuilder<OrderingDbContext>().UseSqlServer(GetDatabaseConnectionString()).Options;
        return new OrderingDbContext(options);
    }
}
