namespace Commerce.Modules.Catalog.Application.DTOs.Responses;

public sealed record ProductVariantOptionDto(
    Guid AttributeId,
    string AttributeName,
    string Value);
