using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.UpdateRole;

public sealed record UpdateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public Guid Id { get; init; }

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = default!;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; }
}
