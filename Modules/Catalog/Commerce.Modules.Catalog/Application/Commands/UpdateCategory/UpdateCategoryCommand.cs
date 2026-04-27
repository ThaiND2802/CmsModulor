using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId,
    int DisplayOrder,
    bool IsActive) : IRequest;
