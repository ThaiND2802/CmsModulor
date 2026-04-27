namespace Commerce.Modules.Catalog.Contracts;

public interface ICatalogVariantLookup
{
    Task<CatalogVariantSummary?> GetByIdAsync(Guid variantId, CancellationToken cancellationToken);
}
