using Commerce.Modules.Inventory.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Commands.ReleaseStock;

public sealed record ReleaseStockCommand(Guid OrderId) : IRequest<ApiResponse<StockReservationDto>>;
