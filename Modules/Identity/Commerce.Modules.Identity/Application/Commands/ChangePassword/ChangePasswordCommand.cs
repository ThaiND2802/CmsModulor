using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand : IRequest<ApiResponse<ChangePasswordResponse>>
{
    public string CurrentPassword { get; init; } = default!;

    public string NewPassword { get; init; } = default!;
}
