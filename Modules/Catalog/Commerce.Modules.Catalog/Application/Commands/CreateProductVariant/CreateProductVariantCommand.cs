using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.CreateProductVariant;

public sealed record CreateProductVariantCommand(
    Guid ProductId,
    string Sku,
    string Name,
    decimal Price,
    decimal? CompareAtPrice,
    int StockQuantity,
    bool IsDefault,
    IReadOnlyList<VariantOptionRequest> Options) : IRequest<Guid>;

public sealed record VariantOptionRequest(Guid AttributeId, string Value);
