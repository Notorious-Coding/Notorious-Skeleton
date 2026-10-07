using NotoriousSkeleton.Libraries.Result;
using NotoriousSkeleton.Ordering.Application;
using NotoriousSkeleton.Ordering.Application.Commands.PlaceOrder;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using NotoriousSkeleton.Ordering.Domain.Order;
using NotoriousSkeleton.Ordering.Integration.Tests.Environment;
using NotoriousSkeleton.Ordering.Integration.Tests.Framework;
using NotoriousSkeleton.Ordering.Integration.Tests.Infrastructure;

using NotoriousTest.XUnit;

using CatalogProduct = NotoriousSkeleton.Catalog.Domain.Product.Product;

namespace NotoriousSkeleton.Ordering.Integration.Tests;

public class OrderIntegrationTest : IntegrationTest<NotoriousSkeletonOrderingEnvironment>
{
    private readonly OrderingArrangeFramework _arrange;
    private readonly OrderingActFramework _act;
    private readonly OrderingAssertFramework _assert;

    public OrderIntegrationTest(XUnitFixture<NotoriousSkeletonOrderingEnvironment> fixture) : base(fixture)
    {
        _act = new OrderingActFramework(Environment);
        _arrange = new OrderingArrangeFramework(Environment);
        _assert = new OrderingAssertFramework(Environment);
    }

    [Fact]
    public async Task PlaceOrder_ShouldCreateOrderWithCatalogPrice()
    {
        CatalogProduct product = await _arrange.AddProduct(new CatalogProduct("Chair", 49.90m));

        Result<OrderReadModel> order = await _act.PlaceOrder(new PlaceOrderCommandParameters { ProductId = product.Id!.Value, Quantity = 2 });

        Assert.True(order.IsSuccess);
        Assert.Equal((int)product.Id!.Value, order.Value!.ProductId);
        Assert.Equal(2, order.Value.Quantity);
        Assert.Equal(49.90m, order.Value.UnitPrice);

        await _assert.ThatOrderIsCreated(order.Value, TestAuthHandler.UserId);
    }

    [Fact]
    public async Task PlaceOrder_ShouldReturnError_WhenProductDoesNotExist()
    {
        Result<OrderReadModel> order = await _act.PlaceOrder(new PlaceOrderCommandParameters { ProductId = 999, Quantity = 1 });

        Assert.True(order.HasErrors);
        Assert.Equal(PlaceOrderErrors.ProductDoesNotExist.Code, Assert.Single(order.Errors).Code);

        await _assert.ThatNoOrderExists();
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnUserOrder()
    {
        Order order = await _arrange.Add(new Order(TestAuthHandler.UserId, 3, 2, 49.90m));

        Result<OrderReadModel> result = await _act.GetOrder(order.Id!.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(order.Id!.Value.Value, result.Value!.Id);
        Assert.Equal(3, result.Value.ProductId);
        Assert.Equal(2, result.Value.Quantity);
        Assert.Equal(49.90m, result.Value.UnitPrice);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnError_WhenOrderBelongsToAnotherUser()
    {
        Order order = await _arrange.Add(new Order("another-user", 3, 2, 49.90m));

        Result<OrderReadModel> result = await _act.GetOrder(order.Id!.Value);

        Assert.True(result.HasErrors);
        Assert.Equal(OrderingErrors.OrderDoesNotExist.Code, Assert.Single(result.Errors).Code);
    }
}
