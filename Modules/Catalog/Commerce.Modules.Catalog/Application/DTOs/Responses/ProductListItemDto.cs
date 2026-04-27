using Commerce.Modules.Catalog.Domain;

namespace Commerce.Modules.Catalog.Application.DTOs.Responses;

public sealed record ProductListItemDto(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    string CategoryName,
    string? BrandName,
    ProductStatus Status,
    bool IsFeatured,
    DateTimeOffset CreatedAtUtc);
