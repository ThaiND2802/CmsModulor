using Commerce.Modules.Inventory.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Queries.GetStockByVariant;

public sealed record GetStockByVariantQuery(Guid VariantId) : IRequest<ApiResponse<InventoryStockDto>>;
