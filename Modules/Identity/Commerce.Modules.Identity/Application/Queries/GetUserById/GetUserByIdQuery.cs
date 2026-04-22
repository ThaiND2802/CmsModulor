using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
{
    public Guid Id { get; init; }
}
