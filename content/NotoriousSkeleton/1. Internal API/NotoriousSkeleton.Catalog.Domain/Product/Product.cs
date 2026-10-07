using NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;

namespace NotoriousSkeleton.Catalog.Domain.Product;

public enum ProductError { NameRequired }

public class Product
{
    public ProductId? Id { get; init; }
    public string Name { get; }
    public Price Price { get; }

    public Product(string name, Price price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException<ProductError>(ProductError.NameRequired, "Product name cannot be empty");

        Name = name;
        Price = price;
    }

    public record struct ProductId(int Value)
    {
        public static implicit operator ProductId(int id) => new ProductId(id);
        public static implicit operator int(ProductId id) => id.Value;
    }
}
