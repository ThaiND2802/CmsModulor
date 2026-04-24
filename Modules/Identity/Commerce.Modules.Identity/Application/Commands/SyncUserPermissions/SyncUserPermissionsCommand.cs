using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.SyncUserPermissions;

public sealed record SyncUserPermissionsCommand : IRequest<ApiResponse<UserPermissionAssignmentsResponse>>
{
    public Guid UserId { get; init; }

    public IReadOnlyList<SyncUserPermissionItem> Permissions { get; init; } = [];
}

public sealed record SyncUserPermissionItem
{
    public Guid PermissionId { get; init; }

    public string Effect { get; init; } = default!;

    public DateTime? ExpiresAtUtc { get; init; }
}
