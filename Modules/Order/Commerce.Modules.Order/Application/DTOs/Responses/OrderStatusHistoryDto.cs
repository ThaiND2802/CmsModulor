namespace Commerce.Modules.Order.Application.DTOs.Responses;

public sealed record OrderStatusHistoryDto(
    Guid Id,
    string? FromStatus,
    string ToStatus,
    string? Note,
    string? ChangedBy,
    DateTime ChangedAtUtc);
