using Commerce.Modules.Inventory.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Commands.AdjustStock;

public sealed record AdjustStockCommand : IRequest<ApiResponse<InventoryStockDto>>
{
    public Guid VariantId { get; init; }

    public int QuantityDelta { get; init; }

    public string Reason { get; init; } = string.Empty;

    public string? ReferenceType { get; init; }

    public Guid? ReferenceId { get; init; }
}
