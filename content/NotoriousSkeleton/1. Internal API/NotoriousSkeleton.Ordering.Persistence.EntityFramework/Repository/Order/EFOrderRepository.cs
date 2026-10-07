using NotoriousSkeleton.Ordering.Application.Providers;
using NotoriousSkeleton.Ordering.Persistence.EntityFramework.Entity;

namespace NotoriousSkeleton.Ordering.Persistence.EntityFramework.Repository.Order;

internal class EFOrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _context;

    public EFOrderRepository(OrderingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Domain.Order.Order> Save(Domain.Order.Order order)
    {
        OrderEntity entity = OrderEntity.FromModel(order);

        _context.Orders.Add(entity);
        await _context.SaveChangesAsync();
        return entity.ToModel();
    }
}
