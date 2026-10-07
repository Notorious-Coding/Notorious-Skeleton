using NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds;
using NotoriousSkeleton.Catalog.Externals.Products;

namespace NotoriousSkeleton.Catalog.Clients;

public class ProductClient : IProductClient
{
    private readonly GetProductsByIdsQueryFeature _getProductsByIdsQueryFeature;

    public ProductClient(GetProductsByIdsQueryFeature getProductsByIdsQueryFeature)
    {
        _getProductsByIdsQueryFeature = getProductsByIdsQueryFeature;
    }

    public async Task<IEnumerable<Product>> GetByIds(IEnumerable<int> ids)
    {
        return (await _getProductsByIdsQueryFeature.Invoke(ids))
            .Select(x => new Product(x.Id, x.Name, x.Price));
    }
}
