using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Commands.CreatePaymentMethod;

public sealed record CreatePaymentMethodCommand : IRequest<ApiResponse<PaymentMethodDto>>
{
    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public bool IsActive { get; init; }
}
