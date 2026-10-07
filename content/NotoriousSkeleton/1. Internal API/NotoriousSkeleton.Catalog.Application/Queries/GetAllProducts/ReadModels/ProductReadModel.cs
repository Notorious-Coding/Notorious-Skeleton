using NotoriousSkeleton.Catalog.Domain.Product;

using System;

namespace NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;

public class ProductReadModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public static ProductReadModel FromDomain(Product product)
    {
        if (product.Id is null)
            throw new InvalidOperationException("Cannot build ProductReadModel from a Product without Id.");

        return new ProductReadModel
        {
            Id = product.Id.Value,
            Name = product.Name,
            Price = product.Price.Value
        };
    }
}
