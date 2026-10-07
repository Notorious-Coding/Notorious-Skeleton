using NotoriousSkeleton.Ordering.Application.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Ordering.Client.Catalog.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingCatalogClients(this IServiceCollection services)
    {
        return services
            .AddScoped<IProductProvider, ProductClient>();
    }
}
