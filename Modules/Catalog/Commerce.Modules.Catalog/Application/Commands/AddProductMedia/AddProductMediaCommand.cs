using Commerce.Modules.Catalog.Domain;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.AddProductMedia;

public sealed record AddProductMediaCommand(
    Guid ProductId,
    string Url,
    string? AltText,
    MediaType MediaType,
    bool IsPrimary) : IRequest<Guid>;
