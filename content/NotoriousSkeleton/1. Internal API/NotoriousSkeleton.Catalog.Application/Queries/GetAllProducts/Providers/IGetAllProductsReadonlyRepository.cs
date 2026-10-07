using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.Providers;

public interface IGetAllProductsReadonlyRepository
{
    public Task<IEnumerable<ProductReadModel>> GetAllProducts();
}
