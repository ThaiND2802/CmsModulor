using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateBrand;

public sealed record UpdateBrandCommand(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string? Website,
    bool IsActive) : IRequest;
