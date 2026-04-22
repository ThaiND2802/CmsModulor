using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.DeleteRole;

public sealed record DeleteRoleCommand : IRequest<ApiResponse<DeleteRoleResponse>>
{
    public Guid Id { get; init; }
}
