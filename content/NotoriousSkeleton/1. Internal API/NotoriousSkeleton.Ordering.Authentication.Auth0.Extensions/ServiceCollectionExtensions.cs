using NotoriousSkeleton.Ordering.Application.Providers;

using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Ordering.Authentication.Auth0.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingUserProvider(this IServiceCollection services)
    {
        services.AddScoped<IUserProvider, Auth0UserProvider>();
        return services;
    }
}
