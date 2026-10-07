using System.Collections.Generic;
using System.Threading.Tasks;
using NotoriousSkeleton.Ordering.Domain.Product;

namespace NotoriousSkeleton.Ordering.Application.Providers;

public interface IProductProvider
{
    public Task<IEnumerable<Product>> GetByIds(IEnumerable<int> ids);
}
