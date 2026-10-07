using FakeItEasy;

using NotoriousSkeleton.Libraries.Result;
using Microsoft.Extensions.Logging;
using NotoriousSkeleton.Ordering.Application;
using NotoriousSkeleton.Ordering.Application.Authentication;
using NotoriousSkeleton.Ordering.Application.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;

namespace NotoriousSkeleton.Ordering.Unit.Tests.Features;

public class GetOrderByIdQueryFeatureUnitTests
{
    private readonly GetOrderByIdQueryFeature _testedFeature;

    private readonly IGetOrderByIdReadonlyRepository _repository;
    private readonly IUserProvider _userProvider;

    public GetOrderByIdQueryFeatureUnitTests()
    {
        _repository = A.Fake<IGetOrderByIdReadonlyRepository>();
        _userProvider = A.Fake<IUserProvider>();
        _testedFeature = new GetOrderByIdQueryFeature(_repository, _userProvider, A.Fake<ILogger<GetOrderByIdQueryFeature>>());

        A.CallTo(() => _userProvider.IsAuthenticated).Returns(true);
        A.CallTo(() => _userProvider.User).Returns(new User("toto"));
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnOrder_WhenItExists()
    {
        Guid orderId = Guid.NewGuid();
        OrderReadModel order = new() { Id = orderId, ProductId = 1, Quantity = 2, UnitPrice = 10 };
        A.CallTo(() => _repository.GetOrderById(orderId, "toto")).Returns(order);

        Result<OrderReadModel> result = await _testedFeature.Invoke(orderId);

        Assert.True(result.IsSuccess);
        Assert.False(result.HasErrors);
        Assert.Same(order, result.Value);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnError_WhenOrderDoesNotExist()
    {
        Guid orderId = Guid.NewGuid();
        A.CallTo(() => _repository.GetOrderById(orderId, "toto")).Returns(Task.FromResult<OrderReadModel?>(null));

        Result<OrderReadModel> result = await _testedFeature.Invoke(orderId);

        Assert.True(result.HasErrors);
        Assert.Single(result.Errors);
        Assert.Equal(OrderingErrors.OrderDoesNotExist.Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task GetOrderById_ShouldThrow_WhenNotAuthenticated()
    {
        A.CallTo(() => _userProvider.IsAuthenticated).Returns(false);
        A.CallTo(() => _userProvider.User).Returns(null as User);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _testedFeature.Invoke(Guid.NewGuid()));
        A.CallTo(() => _repository.GetOrderById(A<Guid>._, A<string>._)).MustNotHaveHappened();
    }
}
