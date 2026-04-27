using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.SubmitSale;

public sealed record SubmitSaleCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid SaleId { get; init; }
}
