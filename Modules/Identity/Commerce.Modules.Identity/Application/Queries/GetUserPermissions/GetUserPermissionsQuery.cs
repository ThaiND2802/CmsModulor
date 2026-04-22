using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetUserPermissions;

public sealed record GetUserPermissionsQuery : IRequest<ApiResponse<UserPermissionAssignmentsResponse>>
{
    public Guid UserId { get; init; }
}
