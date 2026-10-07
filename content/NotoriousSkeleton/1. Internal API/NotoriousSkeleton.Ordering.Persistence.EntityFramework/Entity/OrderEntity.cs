using NotoriousSkeleton.Libraries.Repositories;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using NotoriousSkeleton.Ordering.Domain.Order;

namespace NotoriousSkeleton.Ordering.Persistence.EntityFramework.Entity;

public class OrderEntity : EntityBase<Guid>, IMappable<Order, OrderEntity>
{
    public string UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public static OrderEntity FromModel(Order model)
    {
        return new OrderEntity()
        {
            Id = model.Id?.Value ?? Guid.Empty,
            UserId = model.UserId,
            ProductId = model.ProductId,
            Quantity = model.Quantity,
            UnitPrice = model.UnitPrice,
        };
    }

    public Order ToModel()
    {
        return new Order(UserId, ProductId, Quantity, UnitPrice) { Id = Id };
    }

    public OrderReadModel ToReadModel()
    {
        return new OrderReadModel()
        {
            Id = Id,
            ProductId = ProductId,
            Quantity = Quantity,
            UnitPrice = UnitPrice
        };
    }
}
