using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.CreateUser;

public sealed record CreateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public string UserName { get; init; } = default!;

    public string DisplayName { get; init; } = default!;

    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string Password { get; init; } = default!;

    public bool IsActive { get; init; } = true;
}
