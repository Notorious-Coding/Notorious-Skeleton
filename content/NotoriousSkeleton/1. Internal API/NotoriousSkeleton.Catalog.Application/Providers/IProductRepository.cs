using NotoriousSkeleton.Catalog.Domain.Product;

using System.Threading.Tasks;

namespace NotoriousSkeleton.Catalog.Application.Providers;

public interface IProductRepository
{
    Task<Product> Save(Product product);
}
