using NotoriousSkeleton.Catalog.Application.Commands.AddProduct;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Domain.Product;
using NotoriousSkeleton.Catalog.Integration.Tests.Environment;
using NotoriousSkeleton.Catalog.Integration.Tests.Framework;
using NotoriousSkeleton.Libraries.Result;

using NotoriousTest.XUnit;

namespace NotoriousSkeleton.Catalog.Integration.Tests;

public class ProductIntegrationTest : IntegrationTest<NotoriousSkeletonCatalogEnvironment>
{
    private readonly ArrangeFramework _arrange;
    private readonly ActFramework _act;
    private readonly AssertFramework _assert;
    public ProductIntegrationTest(XUnitFixture<NotoriousSkeletonCatalogEnvironment> fixture) : base(fixture)
    {
        _act = new ActFramework(Environment);
        _arrange = new ArrangeFramework(Environment);
        _assert = new AssertFramework(Environment);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAllProducts()
    {
        List<Product> products = await _arrange.CreateProducts([new Product("Chair", 49.90m), new Product("Table", 129m)]);

        IEnumerable<ProductReadModel> result = await _act.GetAllProducts();

        Assert.Equal(2, result.Count());
        Assert.All(products, p => Assert.Contains(result, r => r.Id == p.Id!.Value.Value && r.Name == p.Name && r.Price == p.Price.Value));
    }

    [Fact]
    public async Task CreateProduct_ShouldCreateProductInDb()
    {
        var parameters = new AddProductCommandParameters("Chair", 49.90m);

        Result<ProductReadModel> product = await _act.Create(parameters);

        Assert.True(product.IsSuccess);
        Assert.Empty(product.Errors);
        Assert.Equal(parameters.Name, product.Value!.Name);
        Assert.Equal(parameters.Price, product.Value.Price);

        await _assert.ThatProductIsCreated(product.Value);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnError_WhenNameIsEmpty()
    {
        Result<ProductReadModel> product = await _act.Create(new AddProductCommandParameters(string.Empty, 49.90m));

        Assert.True(product.HasErrors);
        Assert.Equal(AddProductErrors.NameIsEmpty.Code, Assert.Single(product.Errors).Code);
        Assert.Empty(await _act.GetAllProducts());
    }
}
