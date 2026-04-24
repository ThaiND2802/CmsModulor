using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.UpdateRole;

public sealed record UpdateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public string? Description { get; init; }

    public bool IsActive { get; init; }
}
