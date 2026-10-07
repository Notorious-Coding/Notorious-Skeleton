using FakeItEasy;

using NotoriousSkeleton.Catalog.Application.Authentication;
using NotoriousSkeleton.Catalog.Application.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;

using Microsoft.Extensions.Logging;

namespace NotoriousSkeleton.Catalog.Unit.Tests.Features;

public class GetAllProductsQueryFeatureUnitTests
{
    private readonly GetAllProductsQueryFeature _testedFeature;

    private readonly IGetAllProductsReadonlyRepository _repository;
    private readonly IUserProvider _userProvider;
    private readonly ILogger<GetAllProductsQueryFeature> _logger;

    public GetAllProductsQueryFeatureUnitTests()
    {
        _repository = A.Fake<IGetAllProductsReadonlyRepository>();
        _userProvider = A.Fake<IUserProvider>();
        _logger = A.Fake<ILogger<GetAllProductsQueryFeature>>();
        _testedFeature = new GetAllProductsQueryFeature(_repository, _userProvider, _logger);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnProducts_WhenAuthenticated()
    {
        A.CallTo(() => _userProvider.IsAuthenticated).Returns(true);
        A.CallTo(() => _userProvider.User).Returns(new User("toto"));

        List<ProductReadModel> products =
        [
            new ProductReadModel { Id = 1, Name = "Chair", Price = 49.90m }
        ];
        A.CallTo(() => _repository.GetAllProducts()).Returns(products);

        IEnumerable<ProductReadModel> result = await _testedFeature.Invoke();

        Assert.Equal(products, result);
        A.CallTo(() => _repository.GetAllProducts()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAllProducts_ShouldThrow_WhenNotAuthenticated()
    {
        A.CallTo(() => _userProvider.IsAuthenticated).Returns(false);
        A.CallTo(() => _userProvider.User).Returns(null as User);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _testedFeature.Invoke());
        A.CallTo(() => _repository.GetAllProducts()).MustNotHaveHappened();
    }
}
