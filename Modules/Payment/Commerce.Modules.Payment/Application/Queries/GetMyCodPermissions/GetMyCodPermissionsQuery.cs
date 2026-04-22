using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Queries.GetMyCodPermissions;

public sealed record GetMyCodPermissionsQuery : IRequest<ApiResponse<PaymentCodPermissionsResponse>>;
