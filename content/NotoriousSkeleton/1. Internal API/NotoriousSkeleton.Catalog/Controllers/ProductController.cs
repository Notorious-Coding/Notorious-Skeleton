using Asp.Versioning;

using NotoriousSkeleton.Catalog.Application.Commands.AddProduct;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Libraries.Result;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Net;

namespace NotoriousSkeleton.Catalog.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/product")]
public class ProductController : ControllerBase
{
    private readonly GetAllProductsQueryFeature _getAllProductsQueryFeature;
    private readonly AddProductCommandFeature _addProductCommandFeature;
    private readonly ILogger<ProductController> _logger;

    public ProductController(GetAllProductsQueryFeature getAllProductsQueryFeature, AddProductCommandFeature addProductCommandFeature, ILogger<ProductController> logger)
    {
        _getAllProductsQueryFeature = getAllProductsQueryFeature ?? throw new ArgumentNullException(nameof(getAllProductsQueryFeature));
        _addProductCommandFeature = addProductCommandFeature ?? throw new ArgumentNullException(nameof(addProductCommandFeature));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductReadModel>>> GetAll()
    {
        IEnumerable<ProductReadModel> products = await _getAllProductsQueryFeature.Invoke();

        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<Result<ProductReadModel>>> Create(AddProductCommandParameters @params)
    {
        return StatusCode((int)HttpStatusCode.Created, await _addProductCommandFeature.Invoke(@params));
    }
}
