namespace NotoriousSkeleton.Catalog.Externals.Products;

public interface IProductClient
{
    public Task<IEnumerable<Product>> GetByIds(IEnumerable<int> ids);
}
