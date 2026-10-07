using FakeItEasy;

using NotoriousSkeleton.Catalog.Application.Authentication;
using NotoriousSkeleton.Catalog.Application.Commands.AddProduct;
using NotoriousSkeleton.Catalog.Application.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Domain.Product;
using NotoriousSkeleton.Libraries.Result;

using Microsoft.Extensions.Logging;

namespace NotoriousSkeleton.Catalog.Unit.Tests.Features;

public class AddProductCommandFeatureUnitTests
{
    private readonly AddProductCommandFeature _testedFeature;

    private readonly IProductRepository _productRepository;
    private readonly IUserProvider _userProvider;
    private readonly ILogger<AddProductCommandFeature> _logger;

    public AddProductCommandFeatureUnitTests()
    {
        _productRepository = A.Fake<IProductRepository>();
        _userProvider = A.Fake<IUserProvider>();
        _logger = A.Fake<ILogger<AddProductCommandFeature>>();
        _testedFeature = new AddProductCommandFeature(_productRepository, _userProvider, _logger);
    }

    [Fact]
    public async Task AddProduct_ShouldReturnError_WhenNameIsEmpty()
    {
        A.CallTo(() => _userProvider.IsAuthenticated).Returns(true);
        A.CallTo(() => _userProvider.User).Returns(new User("toto"));

        AddProductCommandParameters @params = new AddProductCommandParameters(string.Empty, 12);
        Result<ProductReadModel> result = await _testedFeature.Invoke(@params);

        Assert.True(result.HasErrors);
        Assert.Single(result.Errors);
        Assert.Null(result.Value);
        Assert.Equal("PRODUCT_NAME_EMPTY", result.Errors[0].Code);
        A.CallTo(() => _productRepository.Save(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddProduct_ShouldReturnError_WhenPriceIsNegative()
    {
        A.CallTo(() => _userProvider.IsAuthenticated).Returns(true);
        A.CallTo(() => _userProvider.User).Returns(new User("toto"));

        AddProductCommandParameters @params = new AddProductCommandParameters("Chair", -1);
        Result<ProductReadModel> result = await _testedFeature.Invoke(@params);

        Assert.True(result.HasErrors);
        Assert.Single(result.Errors);
        Assert.Null(result.Value);
        Assert.Equal("PRODUCT_PRICE_NEGATIVE", result.Errors[0].Code);
        A.CallTo(() => _productRepository.Save(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddProduct_ShouldSaveProduct_WhenParametersAreValid()
    {
        A.CallTo(() => _userProvider.IsAuthenticated).Returns(true);
        A.CallTo(() => _userProvider.User).Returns(new User("toto"));
        A.CallTo(() => _productRepository.Save(A<Product>._))
            .ReturnsLazily((Product product) => new Product(product.Name, product.Price) { Id = 42 });

        AddProductCommandParameters @params = new AddProductCommandParameters("Chair", 49.90m);
        Result<ProductReadModel> result = await _testedFeature.Invoke(@params);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value!.Id);
        Assert.Equal("Chair", result.Value.Name);
        Assert.Equal(49.90m, result.Value.Price);
    }

    [Fact]
    public async Task AddProduct_ShouldThrow_WhenNotAuthenticated()
    {
        A.CallTo(() => _userProvider.IsAuthenticated).Returns(false);
        A.CallTo(() => _userProvider.User).Returns(null as User);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _testedFeature.Invoke(new AddProductCommandParameters("Chair", 12)));
        A.CallTo(() => _productRepository.Save(A<Product>._)).MustNotHaveHappened();
    }
}
