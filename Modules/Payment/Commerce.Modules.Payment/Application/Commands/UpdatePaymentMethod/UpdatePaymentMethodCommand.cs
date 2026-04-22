using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Commands.UpdatePaymentMethod;

public sealed record UpdatePaymentMethodCommand : IRequest<ApiResponse<PaymentMethodDto>>
{
    public Guid Id { get; init; }

    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = default!;

    public bool IsActive { get; init; }
}
