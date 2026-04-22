using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.CreateRole;

public sealed record CreateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    [Required]
    [MaxLength(50)]
    public string Code { get; init; } = default!;

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = default!;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}
