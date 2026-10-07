using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds.Providers;

public interface IGetProductsByIdsReadonlyRepository
{
    Task<IEnumerable<ProductReadModel>> GetByIds(IEnumerable<int> ids);
}
