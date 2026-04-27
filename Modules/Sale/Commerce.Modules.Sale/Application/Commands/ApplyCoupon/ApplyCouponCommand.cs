using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.ApplyCoupon;

public sealed record ApplyCouponCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid SaleId { get; init; }

    public string Code { get; init; } = string.Empty;
}
