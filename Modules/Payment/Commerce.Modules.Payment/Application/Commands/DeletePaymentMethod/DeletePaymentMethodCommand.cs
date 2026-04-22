using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Commands.DeletePaymentMethod;

public sealed record DeletePaymentMethodCommand : IRequest<ApiResponse<DeletePaymentMethodResponse>>
{
    public Guid Id { get; init; }
}
