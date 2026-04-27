using Commerce.Modules.Inventory.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Commands.ReceiveStock;

public sealed record ReceiveStockCommand : IRequest<ApiResponse<InventoryStockDto>>
{
    public Guid VariantId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public string Reason { get; init; } = string.Empty;

    public string? ReferenceType { get; init; }

    public Guid? ReferenceId { get; init; }
}
