using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.CreateUser;

public sealed record CreateUserCommand : IRequest<ApiResponse<UserDto>>
{
    [Required]
    [MaxLength(100)]
    public string UserName { get; init; } = default!;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; init; } = default!;

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(50)]
    public string? PhoneNumber { get; init; }

    [Required]
    [MinLength(8)]
    public string Password { get; init; } = default!;

    public bool IsActive { get; init; } = true;
}
