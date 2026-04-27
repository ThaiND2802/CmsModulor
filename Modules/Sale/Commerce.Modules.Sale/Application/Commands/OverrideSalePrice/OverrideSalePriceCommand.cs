using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.OverrideSalePrice;

public sealed record OverrideSalePriceCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid SaleId { get; init; }

    public Guid ItemId { get; init; }

    public decimal OverridePrice { get; init; }

    public string Reason { get; init; } = string.Empty;
}
