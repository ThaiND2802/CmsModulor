using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.UpdatePermission;

public sealed record UpdatePermissionCommand : IRequest<ApiResponse<PermissionDto>>
{
    public Guid Id { get; init; }

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

    public bool IsActive { get; init; }
}
