using NotoriousSkeleton.Catalog.Application.Common;
using NotoriousSkeleton.Catalog.Application.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Domain.Product;
using NotoriousSkeleton.Libraries.Result;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotoriousSkeleton.Catalog.Application.Commands.AddProduct;

public class AddProductCommandFeature : AuthenticatedFeature
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<AddProductCommandFeature> _logger;

    public AddProductCommandFeature(IProductRepository productRepository,
        IUserProvider userProvider,
        ILogger<AddProductCommandFeature> logger) : base(userProvider)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ProductReadModel>> Invoke(AddProductCommandParameters @params)
    {
        EnsureAuthenticated();

        string userId = User.Id;

        List<Error> errors = Validate(@params);
        if (errors.Count > 0)
        {
            _logger.LogDebug("Product creation rejected: {@Errors}", errors);
            return errors;
        }

        try
        {
            Product product = await _productRepository.Save(new Product(@params.Name, @params.Price));
            _logger.LogInformation("Product created: {ProductId} (user={UserId})", product.Id, userId);

            return ProductReadModel.FromDomain(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Product creation failed (user={UserId}, params={@Params})", userId, @params);
            throw;
        }
    }

    private static List<Error> Validate(AddProductCommandParameters @params)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(@params.Name))
            errors.Add(AddProductErrors.NameIsEmpty);

        if (@params.Price < 0)
            errors.Add(AddProductErrors.PriceNegative);

        return errors;
    }
}
