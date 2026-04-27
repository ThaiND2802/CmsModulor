using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    string Slug,
    string? LogoUrl,
    string? Website,
    bool IsActive = true) : IRequest<Guid>;
