using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Commands.CreatePaymentMethod;

public sealed record CreatePaymentMethodCommand : IRequest<ApiResponse<PaymentMethodDto>>
{
    [Required]
    [MaxLength(50)]
    public string Code { get; init; } = default!;

    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = default!;

    public bool IsActive { get; init; }
}
