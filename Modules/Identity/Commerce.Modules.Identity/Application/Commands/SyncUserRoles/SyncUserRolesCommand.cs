using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.SyncUserRoles;

public sealed record SyncUserRolesCommand : IRequest<ApiResponse<UserRoleAssignmentsResponse>>
{
    public Guid UserId { get; init; }

    [Required]
    public IReadOnlyList<Guid> RoleIds { get; init; } = [];
}
