using NotoriousSkeleton.Catalog.Application.Common;
using NotoriousSkeleton.Catalog.Application.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts;

public class GetAllProductsQueryFeature : AuthenticatedFeature
{
    private readonly IGetAllProductsReadonlyRepository _productRepository;
    private readonly ILogger<GetAllProductsQueryFeature> _logger;

    public GetAllProductsQueryFeature(IGetAllProductsReadonlyRepository productRepository, IUserProvider userProvider, ILogger<GetAllProductsQueryFeature> logger) : base(userProvider)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ProductReadModel>> Invoke()
    {
        EnsureAuthenticated();
        _logger.LogInformation("Retrieving the product list (user={UserId})", User.Id);
        return await _productRepository.GetAllProducts();
    }
}
