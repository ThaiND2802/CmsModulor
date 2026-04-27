namespace Commerce.Modules.Catalog.Contracts;

public sealed record CatalogVariantSummary
{
    public CatalogVariantSummary(Guid id, string sku, bool isActive)
        : this(id, null, sku, string.Empty, null, 0m, isActive)
    {
    }

    public CatalogVariantSummary(
        Guid id,
        Guid? productId,
        string sku,
        string productName,
        string? variantName,
        decimal unitPrice,
        bool isActive)
    {
        Id = id;
        ProductId = productId;
        Sku = sku;
        ProductName = productName;
        VariantName = variantName;
        UnitPrice = unitPrice;
        IsActive = isActive;
    }

    public Guid Id { get; init; }

    public Guid? ProductId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public string? VariantName { get; init; }

    public decimal UnitPrice { get; init; }

    public bool IsActive { get; init; }
}
