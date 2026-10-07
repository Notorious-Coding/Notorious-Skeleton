using NotoriousSkeleton.Catalog.Application.DependencyInjection.Microsoft;
using NotoriousSkeleton.Catalog.Authentication.Auth0.Extensions;
using NotoriousSkeleton.Catalog.Clients;
using NotoriousSkeleton.Catalog.Externals.Products;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Catalog;

public static class ModuleExtensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCatalogFeatures()
            .AddCatalogPersistence(configuration)
            .AddCatalogUserProvider()
            .AddExternals();
    }

    private static IServiceCollection AddExternals(this IServiceCollection services)
    {
        return services
            .AddScoped<IProductClient, ProductClient>();
    }

    public static IMvcBuilder AddCatalogControllers(this IMvcBuilder builder)
    {
        return builder.AddApplicationPart(AssemblyMarker.CurrentAssembly);
    }
}
