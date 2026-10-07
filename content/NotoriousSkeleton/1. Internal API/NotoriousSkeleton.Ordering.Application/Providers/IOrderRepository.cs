using NotoriousSkeleton.Ordering.Domain.Order;

using System.Threading.Tasks;

namespace NotoriousSkeleton.Ordering.Application.Providers;

public interface IOrderRepository
{
    public Task<Order> Save(Order order);
}
