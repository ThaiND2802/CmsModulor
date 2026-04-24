using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand : IRequest<ApiResponse<LoginResponse>>
{
    public string RefreshToken { get; init; } = default!;
}
