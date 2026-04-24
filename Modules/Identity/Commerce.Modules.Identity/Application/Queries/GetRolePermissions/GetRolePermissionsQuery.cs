using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetRolePermissions;

public sealed record GetRolePermissionsQuery : IRequest<ApiResponse<RolePermissionAssignmentsResponse>>
{
    public Guid RoleId { get; init; }
}
