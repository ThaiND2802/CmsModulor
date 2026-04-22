using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.Login;

public sealed record LoginCommand : IRequest<ApiResponse<LoginResponse>>
{
    [Required]
    [MaxLength(100)]
    public string UserName { get; init; } = default!;

    [Required]
    [MaxLength(200)]
    public string Password { get; init; } = default!;
}
