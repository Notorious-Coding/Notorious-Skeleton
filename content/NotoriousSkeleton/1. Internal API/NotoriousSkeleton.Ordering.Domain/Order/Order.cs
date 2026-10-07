using NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;
using NotoriousSkeleton.SharedKernel.User;

namespace NotoriousSkeleton.Ordering.Domain.Order;

public enum OrderError { UserRequired, ProductRequired, QuantityMustBePositive, UnitPriceMustBePositive }

public class Order
{
    public OrderId? Id { get; init; }
    public UserId UserId { get; }
    public int ProductId { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    public Order(UserId userId, int productId, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(userId.Value))
            throw new DomainException<OrderError>(OrderError.UserRequired, "Order user must be set");

        if (productId == default)
            throw new DomainException<OrderError>(OrderError.ProductRequired, "Order product must be set");

        if (quantity <= 0)
            throw new NegativeValueDomainException<OrderError>(OrderError.QuantityMustBePositive, nameof(quantity));

        if (unitPrice < 0)
            throw new NegativeValueDomainException<OrderError>(OrderError.UnitPriceMustBePositive, nameof(unitPrice));

        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public record struct OrderId(Guid Value)
    {
        public static implicit operator OrderId(Guid id) => new OrderId(id);
        public static implicit operator Guid(OrderId id) => id.Value;
    }
}
