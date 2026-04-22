namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record DeletedPaymentMethodDto(
    Guid Id,
    string Code,
    string Name,
    DateTime? DeletedAtUtc,
    string? DeletedBy);
