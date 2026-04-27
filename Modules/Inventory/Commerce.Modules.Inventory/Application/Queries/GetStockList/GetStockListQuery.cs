using Commerce.Modules.Inventory.Application.DTOs.Responses;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Queries.GetStockList;

public sealed class GetStockListQuery : PagedListRequest, IRequest<PagedApiResponse<InventoryStockDto>>
{
}
