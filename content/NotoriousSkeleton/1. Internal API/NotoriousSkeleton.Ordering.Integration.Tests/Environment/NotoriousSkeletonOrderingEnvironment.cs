using System.Reflection;

using NotoriousSkeleton.Ordering.Integration.Tests.Infrastructure;

using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.Web;

namespace NotoriousSkeleton.Ordering.Integration.Tests.Environment;

public class NotoriousSkeletonOrderingEnvironment : EnvironmentBase
{
    public NotoriousSkeletonOrderingEnvironment(EnvironmentSettings settings,
        IWatchDog watchDog,
        IRegistry registry,
        IRuntime runtime,
        ITestLogger logger,
        IServiceProvider serviceProvider) : base(settings, watchDog, registry, runtime, logger, serviceProvider)
    {
    }

    protected override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override Task ConfigureEnvironment()
    {
        // Catalog products are read through the Catalog externals.
        AddInfrastructure<NotoriousSkeletonCatalogDatabase>();
        AddInfrastructure<NotoriousSkeletonOrderingDatabase>();
        this.AddWebApplication<InternalAPIApplication>();
        return Task.CompletedTask;
    }
}
