using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;

using System;
using System.Threading.Tasks;

namespace NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.Providers;

public interface IGetOrderByIdReadonlyRepository
{
    public Task<OrderReadModel?> GetOrderById(Guid id, string userId);
}
