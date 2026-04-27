using Commerce.Modules.Inventory.Application.DTOs.Responses;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Queries.GetStockMovements;

public sealed class GetStockMovementsQuery : PagedListRequest, IRequest<PagedApiResponse<StockMovementDto>>
{
    public Guid VariantId { get; init; }
}
