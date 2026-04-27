using Commerce.Modules.Catalog.Domain;

namespace Commerce.Modules.Catalog.Application.DTOs.Responses;

public sealed record ProductMediaDto(
    Guid Id,
    string Url,
    string? AltText,
    MediaType MediaType,
    int DisplayOrder,
    bool IsPrimary);
