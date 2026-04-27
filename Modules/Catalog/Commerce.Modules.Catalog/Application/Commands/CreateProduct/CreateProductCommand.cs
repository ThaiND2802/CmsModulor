using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Slug,
    string? Description,
    string? ShortDescription,
    string Sku,
    Guid CategoryId,
    Guid? BrandId,
    bool IsFeatured,
    string? Tags) : IRequest<Guid>;
