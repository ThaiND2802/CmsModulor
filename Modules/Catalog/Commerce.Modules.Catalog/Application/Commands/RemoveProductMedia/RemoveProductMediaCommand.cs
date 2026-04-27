using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.RemoveProductMedia;

public sealed record RemoveProductMediaCommand(Guid ProductId, Guid MediaId) : IRequest;
