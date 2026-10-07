using NotoriousSkeleton.Catalog.Externals.Products;
using NotoriousSkeleton.Ordering.Application.Providers;

namespace NotoriousSkeleton.Ordering.Client.Catalog;

internal class ProductClient(IProductClient product) : IProductProvider
{
    public async Task<IEnumerable<NotoriousSkeleton.Ordering.Domain.Product.Product>> GetByIds(IEnumerable<int> ids)
    {
        return (await product.GetByIds(ids))
            .Select(p => new NotoriousSkeleton.Ordering.Domain.Product.Product(p.Id, p.Name, p.Price));
    }
}
