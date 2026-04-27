using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Services;

public sealed class CatalogVariantLookup : ICatalogVariantLookup
{
    private readonly CatalogDbContext _dbContext;

    public CatalogVariantLookup(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<CatalogVariantSummary?> GetByIdAsync(Guid variantId, CancellationToken cancellationToken)
    {
        return await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => x.Id == variantId && !x.IsDeleted)
            .Select(x => new CatalogVariantSummary(
                x.Id,
                x.ProductId,
                x.Sku.Trim(),
                x.Product.Name.Trim(),
                string.IsNullOrWhiteSpace(x.Name) ? null : x.Name.Trim(),
                x.Price,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
