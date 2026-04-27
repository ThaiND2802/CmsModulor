using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateProductVariant;

public sealed record UpdateProductVariantCommand(
    Guid ProductId,
    Guid VariantId,
    string Sku,
    string Name,
    decimal Price,
    decimal? CompareAtPrice,
    int StockQuantity,
    bool IsDefault,
    bool IsActive) : IRequest;
