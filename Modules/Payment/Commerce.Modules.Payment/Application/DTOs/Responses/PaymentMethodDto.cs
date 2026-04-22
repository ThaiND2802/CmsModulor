namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record PaymentMethodDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? UpdatedAtUtc,
    string? UpdatedBy);
