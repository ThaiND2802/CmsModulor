using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteProductVariant;

public sealed record DeleteProductVariantCommand(Guid ProductId, Guid VariantId) : IRequest;
