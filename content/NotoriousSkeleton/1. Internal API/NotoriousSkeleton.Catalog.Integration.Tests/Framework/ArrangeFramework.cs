using NotoriousSkeleton.Catalog.Domain.Product;
using NotoriousSkeleton.Catalog.Integration.Tests.Environment;
using NotoriousSkeleton.Catalog.Integration.Tests.Infrastructure;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework.Entity;

namespace NotoriousSkeleton.Catalog.Integration.Tests.Framework;
internal class ArrangeFramework
{
    private readonly NotoriousSkeletonCatalogEnvironment _environment;
    private readonly CatalogDbContext _dbContext;

    public ArrangeFramework(NotoriousSkeletonCatalogEnvironment environment)
    {
        _environment = environment;

        _dbContext = environment.GetInfrastructure<NotoriousSkeletonCatalogDatabase>().DbContext;
    }

    public async Task<List<Product>> CreateProducts(IEnumerable<Product> products)
    {
        List<ProductEntity> entities = products.Select(ProductEntity.FromModel).ToList();

        _dbContext.Products.AddRange(entities);
        await _dbContext.SaveChangesAsync();

        return entities.Select(e => e.ToModel()).ToList();
    }
}
