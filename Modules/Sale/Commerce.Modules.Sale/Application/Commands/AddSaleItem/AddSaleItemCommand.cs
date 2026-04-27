using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.AddSaleItem;

public sealed record AddSaleItemCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid SaleId { get; init; }

    public Guid VariantId { get; init; }

    public int Quantity { get; init; }

    public decimal DiscountAmount { get; init; }
}
