using System.Reflection;

using NotoriousSkeleton.Catalog.Integration.Tests.Infrastructure;

using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.Web;

namespace NotoriousSkeleton.Catalog.Integration.Tests.Environment;

public class NotoriousSkeletonCatalogEnvironment : EnvironmentBase
{
    public NotoriousSkeletonCatalogEnvironment(EnvironmentSettings settings,
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
        AddInfrastructure<NotoriousSkeletonCatalogDatabase>();
        this.AddWebApplication<InternalAPIApplication>();
        return Task.CompletedTask;
    }
}
