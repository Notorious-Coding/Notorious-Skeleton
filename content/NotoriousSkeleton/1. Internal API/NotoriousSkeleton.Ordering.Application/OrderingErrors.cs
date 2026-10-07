using NotoriousSkeleton.Libraries.Result;

namespace NotoriousSkeleton.Ordering.Application;

public static class OrderingErrors
{
    public static Error OrderDoesNotExist => new("ORDER_NOT_FOUND", "No order found.");
}
