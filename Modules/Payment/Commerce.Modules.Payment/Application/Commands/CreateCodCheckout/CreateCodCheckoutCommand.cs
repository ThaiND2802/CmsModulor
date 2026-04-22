using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Commands.CreateCodCheckout;

public sealed record CreateCodCheckoutCommand : IRequest<ApiResponse<PaymentCodCheckoutResponse>>;
