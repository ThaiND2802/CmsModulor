using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.CreatePermission;

public sealed record CreatePermissionCommand : IRequest<ApiResponse<PermissionDto>>
{
    [Required]
    [MaxLength(150)]
    public string Code { get; init; } = default!;

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = default!;

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Required]
    [MaxLength(100)]
    public string Module { get; init; } = default!;

    [MaxLength(100)]
    public string? Feature { get; init; }

    [Required]
    [MaxLength(100)]
    public string GroupName { get; init; } = default!;

    public int SortOrder { get; init; }

    public bool IsActive { get; init; } = true;
}
