using System;
using System.Threading.Tasks;
using NotoriousSkeleton.Libraries.Result;
using NotoriousSkeleton.Ordering.Application.Common;
using NotoriousSkeleton.Ordering.Application.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;

using Microsoft.Extensions.Logging;

namespace NotoriousSkeleton.Ordering.Application.Queries.GetOrderById;

public class GetOrderByIdQueryFeature : AuthenticatedFeature
{
    private readonly IGetOrderByIdReadonlyRepository _repository;
    private readonly ILogger<GetOrderByIdQueryFeature> _logger;

    public GetOrderByIdQueryFeature(IGetOrderByIdReadonlyRepository repository, IUserProvider userProvider, ILogger<GetOrderByIdQueryFeature> logger) : base(userProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<OrderReadModel>> Invoke(Guid id)
    {
        EnsureAuthenticated();

        OrderReadModel? order = await _repository.GetOrderById(id, User.Id);

        if (order == null)
        {
            _logger.LogDebug("Order not found (order={OrderId}, user={UserId})", id, User.Id);
            return OrderingErrors.OrderDoesNotExist;
        }

        return order;
    }
}
