using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Integration.Tests.Environment;
using NotoriousSkeleton.Catalog.Integration.Tests.Infrastructure;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework.Entity;

using Microsoft.EntityFrameworkCore;

namespace NotoriousSkeleton.Catalog.Integration.Tests.Framework;

internal class AssertFramework
{
    private readonly NotoriousSkeletonCatalogEnvironment _environment;
    private readonly CatalogDbContext _dbContext;
    public AssertFramework(NotoriousSkeletonCatalogEnvironment environment)
    {
        _environment = environment;

        _dbContext = environment.GetInfrastructure<NotoriousSkeletonCatalogDatabase>().DbContext;
    }

    public async Task ThatProductIsCreated(ProductReadModel productReadModel)
    {
        ProductEntity product = await _dbContext.Products.SingleAsync(p => p.Id == productReadModel.Id);

        Assert.NotNull(product);
        Assert.Equal(productReadModel.Id, product.Id);
        Assert.Equal(productReadModel.Name, product.Name);
        Assert.Equal(productReadModel.Price, product.Price);
    }
}
