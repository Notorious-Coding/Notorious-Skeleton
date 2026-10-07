using NotoriousSkeleton.Ordering.Application.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.Providers;
using NotoriousSkeleton.Ordering.Persistence.EntityFramework.Repository.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Ordering.Persistence.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<IOrderRepository, EFOrderRepository>()
            .AddScoped<IGetOrderByIdReadonlyRepository, EFOrderReadonlyRepository>()
            .AddDbContext<OrderingDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("Ordering"),
                    sqlServer => sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"));
            });
    }
}
