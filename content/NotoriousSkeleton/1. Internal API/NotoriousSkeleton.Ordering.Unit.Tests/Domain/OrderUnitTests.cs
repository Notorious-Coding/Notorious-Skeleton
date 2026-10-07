using NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;
using NotoriousSkeleton.Ordering.Domain.Order;

namespace NotoriousSkeleton.Ordering.Unit.Tests.Domain;

public class OrderUnitTests
{
    [Fact]
    public void Order_ShouldBeCreated_WhenValuesAreValid()
    {
        var order = new Order("toto", 3, 2, 49.90m);

        Assert.Equal("toto", order.UserId.Value);
        Assert.Equal(3, order.ProductId);
        Assert.Equal(2, order.Quantity);
        Assert.Equal(49.90m, order.UnitPrice);
    }

    [Fact]
    public void Order_ShouldThrow_WhenUserIsEmpty()
    {
        var exception = Assert.Throws<DomainException<OrderError>>(() => new Order("", 3, 2, 10));
        Assert.Equal(OrderError.UserRequired, exception.Error);
    }

    [Fact]
    public void Order_ShouldThrow_WhenProductIsNotSet()
    {
        var exception = Assert.Throws<DomainException<OrderError>>(() => new Order("toto", default, 2, 10));
        Assert.Equal(OrderError.ProductRequired, exception.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Order_ShouldThrow_WhenQuantityIsNotPositive(int quantity)
    {
        var exception = Assert.Throws<NegativeValueDomainException<OrderError>>(() => new Order("toto", 3, quantity, 10));
        Assert.Equal(OrderError.QuantityMustBePositive, exception.Error);
    }
}
