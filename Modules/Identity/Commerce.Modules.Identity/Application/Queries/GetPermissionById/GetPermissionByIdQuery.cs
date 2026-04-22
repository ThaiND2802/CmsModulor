using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetPermissionById;

public sealed record GetPermissionByIdQuery : IRequest<ApiResponse<PermissionDto>>
{
    public Guid Id { get; init; }
}
