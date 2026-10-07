using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace NotoriousSkeleton.Ordering.Persistence.EntityFramework.Repository.Order;

internal class EFOrderReadonlyRepository : IGetOrderByIdReadonlyRepository
{
    private readonly OrderingDbContext _context;

    public EFOrderReadonlyRepository(OrderingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OrderReadModel?> GetOrderById(Guid id, string userId)
    {
        return await _context.Orders.AsNoTracking()
            .Where(o => o.Id == id && o.UserId == userId)
            .Select(o => o.ToReadModel())
            .SingleOrDefaultAsync();
    }
}
