using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.CreatePermission;

public sealed record CreatePermissionCommand : IRequest<ApiResponse<PermissionDto>>
{
    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string? Description { get; init; }

    public string Module { get; init; } = default!;

    public string? Feature { get; init; }

    public string GroupName { get; init; } = default!;

    public int SortOrder { get; init; }

    public bool IsActive { get; init; } = true;
}
