using Commerce.Modules.Catalog.Domain;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.ChangeProductStatus;

public sealed record ChangeProductStatusCommand(Guid Id, ProductStatus Status) : IRequest;
