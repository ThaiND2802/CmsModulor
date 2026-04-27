using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : IRequest;
