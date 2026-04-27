namespace Commerce.Modules.Catalog.Application.DTOs.Responses;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<CategoryDto> Children);
