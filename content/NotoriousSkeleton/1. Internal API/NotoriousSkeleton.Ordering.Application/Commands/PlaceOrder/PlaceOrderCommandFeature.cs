using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotoriousSkeleton.Libraries.Result;
using NotoriousSkeleton.Ordering.Application.Common;
using NotoriousSkeleton.Ordering.Application.Providers;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using NotoriousSkeleton.Ordering.Domain.Order;
using NotoriousSkeleton.Ordering.Domain.Product;
using Microsoft.Extensions.Logging;

namespace NotoriousSkeleton.Ordering.Application.Commands.PlaceOrder;

public class PlaceOrderCommandFeature : AuthenticatedFeature
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductProvider _productProvider;
    private readonly ILogger<PlaceOrderCommandFeature> _logger;

    public PlaceOrderCommandFeature(IOrderRepository orderRepository,
        IProductProvider productProvider,
        IUserProvider userProvider,
        ILogger<PlaceOrderCommandFeature> logger) : base(userProvider)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productProvider = productProvider ?? throw new ArgumentNullException(nameof(productProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<OrderReadModel>> Invoke(PlaceOrderCommandParameters @params)
    {
        EnsureAuthenticated();

        string userId = User.Id;

        IEnumerable<Error> errors = Validate(@params);
        if (errors.Any())
        {
            _logger.LogDebug("Order placement rejected (user={UserId}): {@Errors}", userId, errors);
            return errors.ToList();
        }

        Product? product = (await _productProvider.GetByIds([@params.ProductId])).SingleOrDefault();
        if (product == null)
        {
            _logger.LogDebug("Order placement rejected, product not found (product={ProductId}, user={UserId})", @params.ProductId, userId);
            return PlaceOrderErrors.ProductDoesNotExist;
        }

        try
        {
            Order order = await _orderRepository.Save(new Order(userId, product.Id, @params.Quantity, product.Price));
            _logger.LogInformation("Order placed: {OrderId} (user={UserId})", order.Id, userId);

            return OrderReadModel.FromDomain(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order placement failed (user={UserId}, params={@Params})", userId, @params);
            throw;
        }
    }

    private static IEnumerable<Error> Validate(PlaceOrderCommandParameters @params)
    {
        if (@params.ProductId == default)
        {
            yield return PlaceOrderErrors.ProductRequired;
        }

        if (@params.Quantity <= 0)
        {
            yield return PlaceOrderErrors.QuantityMustBePositive;
        }
    }
}
