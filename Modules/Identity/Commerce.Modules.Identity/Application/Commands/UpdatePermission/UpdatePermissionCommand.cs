using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.UpdatePermission;

public sealed record UpdatePermissionCommand : IRequest<ApiResponse<PermissionDto>>
{
    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public string? Description { get; init; }

    public string Module { get; init; } = default!;

    public string? Feature { get; init; }

    public string GroupName { get; init; } = default!;

    public int SortOrder { get; init; }

    public bool IsActive { get; init; }
}
