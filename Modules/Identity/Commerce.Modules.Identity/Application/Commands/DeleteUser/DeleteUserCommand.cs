using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.DeleteUser;

public sealed record DeleteUserCommand : IRequest<ApiResponse<DeleteUserResponse>>
{
    public Guid Id { get; init; }
}
