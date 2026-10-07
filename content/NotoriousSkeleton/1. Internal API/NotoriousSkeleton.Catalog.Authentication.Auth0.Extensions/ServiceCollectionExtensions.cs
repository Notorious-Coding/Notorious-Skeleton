using NotoriousSkeleton.Catalog.Application.Providers;

using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Catalog.Authentication.Auth0.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogUserProvider(this IServiceCollection services)
    {
        services.AddScoped<IUserProvider, Auth0UserProvider>();
        return services;
    }
}
