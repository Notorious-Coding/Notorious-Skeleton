using NotoriousSkeleton.Libraries.Result;

namespace NotoriousSkeleton.Ordering.Application.Commands.PlaceOrder;

public static class PlaceOrderErrors
{
    public static Error ProductRequired => new Error("PRODUCT_REQUIRED", "Product must be provided", nameof(PlaceOrderCommandParameters.ProductId));
    public static Error ProductDoesNotExist => new Error("PRODUCT_NOT_FOUND", "The specified product does not exist.", nameof(PlaceOrderCommandParameters.ProductId));
    public static Error QuantityMustBePositive => new Error("QUANTITY_MUST_BE_POSITIVE", "Quantity must be greater than zero", nameof(PlaceOrderCommandParameters.Quantity));
}
