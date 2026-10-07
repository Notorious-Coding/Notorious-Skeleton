using NotoriousSkeleton.Libraries.Result;

namespace NotoriousSkeleton.Catalog.Application.Commands.AddProduct;

public static class AddProductErrors
{
    public static Error NameIsEmpty => new Error("PRODUCT_NAME_EMPTY", "Name cannot be empty", nameof(AddProductCommandParameters.Name));
    public static Error PriceNegative => new Error("PRODUCT_PRICE_NEGATIVE", "Price must be greater or equal than zero", nameof(AddProductCommandParameters.Price));
}
