using NotoriousSkeleton.Catalog.Application.Commands.AddProduct;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts;
using NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds;
using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Catalog.Application.DependencyInjection.Microsoft;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogFeatures(this IServiceCollection services)
    {
        return services
            .AddScoped<GetAllProductsQueryFeature>()
            .AddScoped<GetProductsByIdsQueryFeature>()
            .AddScoped<AddProductCommandFeature>();
    }
}
