using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Queries.GetCodHealth;

public sealed record GetCodHealthQuery : IRequest<ApiResponse<PaymentCodHealthResponse>>;
