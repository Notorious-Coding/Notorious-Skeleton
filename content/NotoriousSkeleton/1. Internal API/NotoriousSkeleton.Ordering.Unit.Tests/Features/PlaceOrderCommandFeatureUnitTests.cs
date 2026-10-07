using FakeItEasy;

using NotoriousSkeleton.Libraries.Result;
using Microsoft.Extensions.Logging;
using NotoriousSkeleton.Ordering.Application.Authentication;
using NotoriousSkeleton.Ordering.Application.Commands.PlaceOrder;
using NotoriousSkeleton.Ordering.Application.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using NotoriousSkeleton.Ordering.Domain.Order;
using NotoriousSkeleton.Ordering.Domain.Product;

namespace NotoriousSkeleton.Ordering.Unit.Tests.Features;

public class PlaceOrderCommandFeatureUnitTests
{
    private readonly PlaceOrderCommandFeature _testedFeature;

    private readonly IOrderRepository _orderRepository;
    private readonly IProductProvider _productProvider;
    private readonly IUserProvider _userProvider;

    public PlaceOrderCommandFeatureUnitTests()
    {
        _orderRepository = A.Fake<IOrderRepository>();
        _productProvider = A.Fake<IProductProvider>();
        _userProvider = A.Fake<IUserProvider>();
        _testedFeature = new PlaceOrderCommandFeature(_orderRepository, _productProvider, _userProvider, A.Fake<ILogger<PlaceOrderCommandFeature>>());

        A.CallTo(() => _userProvider.IsAuthenticated).Returns(true);
        A.CallTo(() => _userProvider.User).Returns(new User("toto"));
    }

    [Fact]
    public async Task PlaceOrder_ShouldReturnErrors_WhenParametersAreInvalid()
    {
        Result<OrderReadModel> result = await _testedFeature.Invoke(new PlaceOrderCommandParameters { ProductId = default, Quantity = 0 });

        Assert.True(result.HasErrors);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Code == PlaceOrderErrors.ProductRequired.Code);
        Assert.Contains(result.Errors, e => e.Code == PlaceOrderErrors.QuantityMustBePositive.Code);
        A.CallTo(() => _orderRepository.Save(A<Order>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task PlaceOrder_ShouldReturnError_WhenProductDoesNotExist()
    {
        A.CallTo(() => _productProvider.GetByIds(A<IEnumerable<int>>._)).Returns(Enumerable.Empty<Product>());

        Result<OrderReadModel> result = await _testedFeature.Invoke(new PlaceOrderCommandParameters { ProductId = 14, Quantity = 1 });

        Assert.True(result.HasErrors);
        Assert.Single(result.Errors);
        Assert.Equal(PlaceOrderErrors.ProductDoesNotExist.Code, result.Errors[0].Code);
        A.CallTo(() => _orderRepository.Save(A<Order>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task PlaceOrder_ShouldSaveOrderWithProductPrice()
    {
        Guid orderId = Guid.NewGuid();
        A.CallTo(() => _productProvider.GetByIds(A<IEnumerable<int>>._)).Returns([new Product(3, "Chair", 49.90m)]);
        A.CallTo(() => _orderRepository.Save(A<Order>._))
            .ReturnsLazily((Order order) => new Order(order.UserId, order.ProductId, order.Quantity, order.UnitPrice) { Id = orderId });

        Result<OrderReadModel> result = await _testedFeature.Invoke(new PlaceOrderCommandParameters { ProductId = 3, Quantity = 2 });

        Assert.True(result.IsSuccess);
        Assert.Equal(orderId, result.Value!.Id);
        Assert.Equal(3, result.Value.ProductId);
        Assert.Equal(2, result.Value.Quantity);
        Assert.Equal(49.90m, result.Value.UnitPrice);
        A.CallTo(() => _orderRepository.Save(A<Order>.That.Matches(o => o.UserId == "toto"))).MustHaveHappenedOnceExactly();
    }
}
