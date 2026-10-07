using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Domain.Product;
using NotoriousSkeleton.Libraries.Repositories;

namespace NotoriousSkeleton.Catalog.Persistence.EntityFramework.Entity;

public class ProductEntity : EntityBase<int>, IMappable<Product, ProductEntity>
{
    public string Name { get; set; }
    public decimal Price { get; set; }

    public static ProductEntity FromModel(Product model)
    {
        return new ProductEntity()
        {
            Id = model.Id?.Value ?? default,
            Name = model.Name,
            Price = model.Price,
        };
    }

    public Product ToModel()
    {
        return new Product(Name, Price) { Id = Id };
    }

    public ProductReadModel ToReadModel()
    {
        return new ProductReadModel()
        {
            Id = Id,
            Name = Name,
            Price = Price
        };
    }
}
