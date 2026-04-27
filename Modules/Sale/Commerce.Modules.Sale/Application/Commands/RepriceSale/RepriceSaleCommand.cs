using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.RepriceSale;

public sealed record RepriceSaleCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid SaleId { get; init; }

    public decimal DiscountAmount { get; init; }

    public decimal ShippingAmount { get; init; }

    public decimal TaxAmount { get; init; }
}
