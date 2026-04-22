using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetRoleById;

public sealed record GetRoleByIdQuery : IRequest<ApiResponse<RoleDto>>
{
    public Guid Id { get; init; }
}
