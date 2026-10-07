using FakeItEasy;

using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds;
using NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds.Providers;

namespace NotoriousSkeleton.Catalog.Unit.Tests.Features;

public class GetProductsByIdsQueryFeatureUnitTests
{
    private readonly GetProductsByIdsQueryFeature _testedFeature;

    private readonly IGetProductsByIdsReadonlyRepository _repository;

    public GetProductsByIdsQueryFeatureUnitTests()
    {
        _repository = A.Fake<IGetProductsByIdsReadonlyRepository>();
        _testedFeature = new GetProductsByIdsQueryFeature(_repository);
    }

    [Fact]
    public async Task GetProductsByIds_ShouldReturnRequestedProducts()
    {
        int[] ids = [1, 2];
        List<ProductReadModel> products =
        [
            new ProductReadModel { Id = 1, Name = "Chair", Price = 49.90m },
            new ProductReadModel { Id = 2, Name = "Table", Price = 129m }
        ];
        A.CallTo(() => _repository.GetByIds(ids)).Returns(products);

        IEnumerable<ProductReadModel> result = await _testedFeature.Invoke(ids);

        Assert.Equal(products, result);
        A.CallTo(() => _repository.GetByIds(ids)).MustHaveHappenedOnceExactly();
    }
}
