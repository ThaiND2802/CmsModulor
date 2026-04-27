using Commerce.Modules.Catalog.Domain;

namespace Commerce.Modules.Catalog.Application.DTOs.Responses;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ShortDescription,
    string Sku,
    Guid CategoryId,
    string CategoryName,
    Guid? BrandId,
    string? BrandName,
    ProductStatus Status,
    bool IsFeatured,
    string? Tags,
    IReadOnlyList<ProductMediaDto> Media,
    IReadOnlyList<ProductVariantDto> Variants,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
