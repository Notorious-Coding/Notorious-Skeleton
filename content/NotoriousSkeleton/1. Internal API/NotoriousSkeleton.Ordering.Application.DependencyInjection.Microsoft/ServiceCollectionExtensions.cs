using NotoriousSkeleton.Ordering.Application.Commands.PlaceOrder;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById;
using Microsoft.Extensions.DependencyInjection;

namespace NotoriousSkeleton.Ordering.Application.DependencyInjection.Microsoft;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingFeatures(this IServiceCollection services)
    {
            return services.AddScoped<PlaceOrderCommandFeature>()
                .AddScoped<GetOrderByIdQueryFeature>();
    }
}
