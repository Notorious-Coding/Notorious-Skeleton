using NotoriousSkeleton.Catalog.Application.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds.Providers;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework.Repository.Product;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace NotoriousSkeleton.Catalog.Persistence.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<IGetAllProductsReadonlyRepository, EFProductReadonlyRepository>()
            .AddScoped<IGetProductsByIdsReadonlyRepository, EFProductReadonlyRepository>()
            .AddScoped<IProductRepository, EFProductRepository>()
            .AddDbContext<CatalogDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("Catalog"),
                    sqlServer => sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"));
            });
    }
}
