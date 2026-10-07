using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds.Providers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds;

public class GetProductsByIdsQueryFeature(IGetProductsByIdsReadonlyRepository repository)
{
    private readonly IGetProductsByIdsReadonlyRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<ProductReadModel>> Invoke(IEnumerable<int> ids)
    {
        return await _repository.GetByIds(ids);
    }
}
