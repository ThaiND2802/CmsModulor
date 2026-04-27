using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.ValidateSaleStock;

public sealed record ValidateSaleStockCommand : IRequest<ApiResponse<SaleStockValidationDto>>
{
    public Guid SaleId { get; init; }
}
