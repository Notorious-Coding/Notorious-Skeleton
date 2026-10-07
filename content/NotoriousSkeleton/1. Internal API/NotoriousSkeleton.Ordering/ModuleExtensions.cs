using NotoriousSkeleton.Ordering.Application.DependencyInjection.Microsoft;
using NotoriousSkeleton.Ordering.Authentication.Auth0.Extensions;
using NotoriousSkeleton.Ordering.Client.Catalog.Extensions;
using NotoriousSkeleton.Ordering.Persistence.EntityFramework.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Ordering;

public static class ModuleExtensions
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddOrderingFeatures()
                       .AddOrderingPersistence(configuration)
                       .AddOrderingUserProvider()
                       .AddOrderingCatalogClients();
    }

    public static IMvcBuilder AddOrderingControllers(this IMvcBuilder builder)
    {
        return builder.AddApplicationPart(AssemblyMarker.CurrentAssembly);
    }
}
