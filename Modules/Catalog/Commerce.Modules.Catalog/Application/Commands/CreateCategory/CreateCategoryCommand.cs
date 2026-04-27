using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId,
    int DisplayOrder) : IRequest<Guid>;
