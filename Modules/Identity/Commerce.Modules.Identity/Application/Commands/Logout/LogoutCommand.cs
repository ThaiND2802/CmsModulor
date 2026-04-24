using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.Logout;

public sealed record LogoutCommand : IRequest<ApiResponse<LogoutResponse>>
{
    public string RefreshToken { get; init; } = default!;
}
