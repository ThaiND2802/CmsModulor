using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public Guid Id { get; init; }

    public string DisplayName { get; init; } = default!;

    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }

    public string? Password { get; init; }
}
