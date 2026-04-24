using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.Login;

public sealed record LoginCommand : IRequest<ApiResponse<LoginResponse>>
{
    public string UserName { get; init; } = default!;

    public string Password { get; init; } = default!;
}
