using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.SyncRolePermissions;

public sealed record SyncRolePermissionsCommand : IRequest<ApiResponse<RolePermissionAssignmentsResponse>>
{
    public Guid RoleId { get; init; }

    public IReadOnlyList<SyncRolePermissionItem> Permissions { get; init; } = [];
}

public sealed record SyncRolePermissionItem
{
    public Guid PermissionId { get; init; }

    public string Effect { get; init; } = default!;
}
