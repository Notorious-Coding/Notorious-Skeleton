using Asp.Versioning;

using NotoriousSkeleton.Libraries.Result;
using NotoriousSkeleton.Ordering.Application.Commands.PlaceOrder;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net;

namespace NotoriousSkeleton.Ordering.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/order")]
public class OrderController : ControllerBase
{
    private readonly PlaceOrderCommandFeature _placeOrderCommandFeature;
    private readonly GetOrderByIdQueryFeature _getOrderByIdQueryFeature;

    public OrderController(PlaceOrderCommandFeature placeOrderCommandFeature,
        GetOrderByIdQueryFeature getOrderByIdQueryFeature)
    {
        _placeOrderCommandFeature = placeOrderCommandFeature ?? throw new ArgumentNullException(nameof(placeOrderCommandFeature));
        _getOrderByIdQueryFeature = getOrderByIdQueryFeature ?? throw new ArgumentNullException(nameof(getOrderByIdQueryFeature));
    }

    [HttpPost]
    public async Task<ActionResult<Result<OrderReadModel>>> PlaceOrder(PlaceOrderCommandParameters @params)
    {
        return StatusCode((int)HttpStatusCode.Created, await _placeOrderCommandFeature.Invoke(@params));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<OrderReadModel>>> GetOrder(Guid id)
    {
        return Ok(await _getOrderByIdQueryFeature.Invoke(id));
    }
}
