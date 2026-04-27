namespace Commerce.Modules.Catalog.Application.DTOs.Responses;

public sealed record BrandDto(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string? Website,
    bool IsActive);
