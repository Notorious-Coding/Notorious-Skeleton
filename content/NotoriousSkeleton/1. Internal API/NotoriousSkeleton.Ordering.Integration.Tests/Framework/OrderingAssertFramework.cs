using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using NotoriousSkeleton.Ordering.Integration.Tests.Environment;
using NotoriousSkeleton.Ordering.Integration.Tests.Infrastructure;
using NotoriousSkeleton.Ordering.Persistence.EntityFramework;
using NotoriousSkeleton.Ordering.Persistence.EntityFramework.Entity;

using Microsoft.EntityFrameworkCore;

namespace NotoriousSkeleton.Ordering.Integration.Tests.Framework;

internal class OrderingAssertFramework
{
    private readonly NotoriousSkeletonOrderingEnvironment _environment;
    private readonly OrderingDbContext _dbContext;

    public OrderingAssertFramework(NotoriousSkeletonOrderingEnvironment environment)
    {
        _environment = environment;

        _dbContext = environment.GetInfrastructure<NotoriousSkeletonOrderingDatabase>().DbContext;
    }

    public async Task ThatOrderIsCreated(OrderReadModel expected, string userId)
    {
        OrderEntity order = await _dbContext.Orders.AsNoTracking().SingleAsync(o => o.Id == expected.Id);

        Assert.Equal(userId, order.UserId);
        Assert.Equal(expected.ProductId, order.ProductId);
        Assert.Equal(expected.Quantity, order.Quantity);
        Assert.Equal(expected.UnitPrice, order.UnitPrice);
    }

    public async Task ThatNoOrderExists()
    {
        Assert.False(await _dbContext.Orders.AsNoTracking().AnyAsync());
    }
}
