namespace NotoriousSkeleton.Catalog.Application.Commands.AddProduct;

public class AddProductCommandParameters
{
    public string Name { get; }
    public decimal Price { get; }

    public AddProductCommandParameters(string Name, decimal Price)
    {
        this.Name = Name;
        this.Price = Price;
    }
}
