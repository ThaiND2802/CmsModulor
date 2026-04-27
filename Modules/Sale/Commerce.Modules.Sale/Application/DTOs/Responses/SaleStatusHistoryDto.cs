namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleStatusHistoryDto(
    Guid Id,
    string? FromStatus,
    string ToStatus,
    string? Note,
    string? ChangedBy,
    DateTime ChangedAtUtc);
