using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.Providers;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Application.Queries.GetProductsByIds.Providers;
using Microsoft.EntityFrameworkCore;

namespace NotoriousSkeleton.Catalog.Persistence.EntityFramework.Repository.Product;

internal class EFProductReadonlyRepository : IGetAllProductsReadonlyRepository, IGetProductsByIdsReadonlyRepository
{
    private readonly CatalogDbContext _context;

    public EFProductReadonlyRepository(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<ProductReadModel>> GetAllProducts()
    {
        return await _context.Products.AsNoTracking().Select(p => p.ToReadModel()).ToListAsync();
    }

    public async Task<IEnumerable<ProductReadModel>> GetByIds(IEnumerable<int> ids)
    {
        return await _context.Products.AsNoTracking().Where(p => ids.Contains(p.Id)).Select(p => p.ToReadModel()).ToListAsync();
    }
}
