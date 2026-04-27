using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ShortDescription,
    string Sku,
    Guid CategoryId,
    Guid? BrandId,
    bool IsFeatured,
    string? Tags) : IRequest;
