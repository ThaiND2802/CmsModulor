using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.SyncUserPermissions;

public sealed record SyncUserPermissionsCommand : IRequest<ApiResponse<UserPermissionAssignmentsResponse>>
{
    public Guid UserId { get; init; }

    [Required]
    public IReadOnlyList<SyncUserPermissionItem> Permissions { get; init; } = [];
}

public sealed record SyncUserPermissionItem
{
    [Required]
    public Guid PermissionId { get; init; }

    [Required]
    [RegularExpression("Allow|Deny")]
    public string Effect { get; init; } = default!;

    public DateTime? ExpiresAtUtc { get; init; }
}
