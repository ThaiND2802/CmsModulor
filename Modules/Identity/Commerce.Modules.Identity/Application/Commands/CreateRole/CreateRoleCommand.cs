using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.CreateRole;

public sealed record CreateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}
