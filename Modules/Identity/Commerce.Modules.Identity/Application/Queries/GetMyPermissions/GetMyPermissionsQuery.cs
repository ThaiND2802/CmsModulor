using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetMyPermissions;

public sealed record GetMyPermissionsQuery : IRequest<ApiResponse<MyPermissionsResponse>>;
