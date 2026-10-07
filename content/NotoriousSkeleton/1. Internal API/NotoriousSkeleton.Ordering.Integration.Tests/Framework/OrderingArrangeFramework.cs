using NotoriousSkeleton.Catalog.Persistence.EntityFramework;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework.Entity;
using NotoriousSkeleton.Ordering.Domain.Order;
using NotoriousSkeleton.Ordering.Integration.Tests.Environment;
using NotoriousSkeleton.Ordering.Integration.Tests.Infrastructure;
using NotoriousSkeleton.Ordering.Persistence.EntityFramework;
using NotoriousSkeleton.Ordering.Persistence.EntityFramework.Entity;

namespace NotoriousSkeleton.Ordering.Integration.Tests.Framework;

internal class OrderingArrangeFramework
{
    private readonly NotoriousSkeletonOrderingEnvironment _environment;
    private readonly OrderingDbContext _dbContext;
    private readonly CatalogDbContext _catalogDbContext;

    public OrderingArrangeFramework(NotoriousSkeletonOrderingEnvironment environment)
    {
        _environment = environment;

        _dbContext = environment.GetInfrastructure<NotoriousSkeletonOrderingDatabase>().DbContext;
        _catalogDbContext = environment.GetInfrastructure<NotoriousSkeletonCatalogDatabase>().DbContext;
    }

    public async Task<Catalog.Domain.Product.Product> AddProduct(Catalog.Domain.Product.Product product)
    {
        var entity = ProductEntity.FromModel(product);

        _catalogDbContext.Products.Add(entity);
        await _catalogDbContext.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task<Order> Add(Order order)
    {
        var entity = OrderEntity.FromModel(order);

        _dbContext.Orders.Add(entity);
        await _dbContext.SaveChangesAsync();

        return entity.ToModel();
    }
}
