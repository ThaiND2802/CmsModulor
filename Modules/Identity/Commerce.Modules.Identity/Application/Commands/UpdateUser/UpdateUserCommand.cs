using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public Guid Id { get; init; }

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; init; } = default!;

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(50)]
    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }

    [MinLength(8)]
    public string? Password { get; init; }
}
