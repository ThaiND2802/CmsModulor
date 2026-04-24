using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.ResetUserPassword;

public sealed record ResetUserPasswordCommand : IRequest<ApiResponse<ResetUserPasswordResponse>>
{
    public Guid Id { get; init; }

    public string NewPassword { get; init; } = default!;
}
