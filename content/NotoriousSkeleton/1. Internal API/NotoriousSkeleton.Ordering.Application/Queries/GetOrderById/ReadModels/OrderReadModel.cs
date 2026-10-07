using NotoriousSkeleton.Ordering.Domain.Order;

using System;

namespace NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;

public class OrderReadModel
{
    public Guid Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public static OrderReadModel FromDomain(Order order)
    {
        if (order.Id is null)
            throw new InvalidOperationException("Cannot build OrderReadModel from an Order without Id.");

        return new OrderReadModel
        {
            Id = order.Id.Value,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            UnitPrice = order.UnitPrice
        };
    }
}
