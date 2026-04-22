using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Queries.GetPaymentMethodById;

public sealed record GetPaymentMethodByIdQuery : IRequest<ApiResponse<PaymentMethodDto>>
{
    public Guid Id { get; init; }
}
