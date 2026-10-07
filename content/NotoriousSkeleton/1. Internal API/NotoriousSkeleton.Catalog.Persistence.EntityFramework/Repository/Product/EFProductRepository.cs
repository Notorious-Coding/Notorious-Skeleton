using NotoriousSkeleton.Catalog.Application.Providers;
using NotoriousSkeleton.Catalog.Persistence.EntityFramework.Entity;

namespace NotoriousSkeleton.Catalog.Persistence.EntityFramework.Repository.Product;

internal class EFProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public EFProductRepository(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Domain.Product.Product> Save(Domain.Product.Product product)
    {
        ProductEntity entity = ProductEntity.FromModel(product);

        _context.Add(entity);
        await _context.SaveChangesAsync();
        return entity.ToModel();
    }
}
