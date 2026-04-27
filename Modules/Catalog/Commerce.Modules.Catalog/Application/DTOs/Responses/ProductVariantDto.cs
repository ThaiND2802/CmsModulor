namespace Commerce.Modules.Catalog.Application.DTOs.Responses;

public sealed record ProductVariantDto(
    Guid Id,
    string Sku,
    string Name,
    decimal Price,
    decimal? CompareAtPrice,
    int StockQuantity,
    bool IsDefault,
    bool IsActive,
    IReadOnlyList<ProductVariantOptionDto> Options);
