using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.DeletePermission;

public sealed record DeletePermissionCommand : IRequest<ApiResponse<DeletePermissionResponse>>
{
    public Guid Id { get; init; }
}
