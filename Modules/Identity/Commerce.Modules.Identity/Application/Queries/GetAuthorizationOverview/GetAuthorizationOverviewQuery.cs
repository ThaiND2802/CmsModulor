using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetAuthorizationOverview;

public sealed record GetAuthorizationOverviewQuery
    : IRequest<ApiResponse<AuthorizationOverviewDto>>;
