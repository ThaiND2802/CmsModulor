using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.CancelSale;

public sealed record CancelSaleCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid SaleId { get; init; }

    public string? Reason { get; init; }
}
