using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetUserRoles;

public sealed record GetUserRolesQuery : IRequest<ApiResponse<UserRoleAssignmentsResponse>>
{
    public Guid UserId { get; init; }
}
