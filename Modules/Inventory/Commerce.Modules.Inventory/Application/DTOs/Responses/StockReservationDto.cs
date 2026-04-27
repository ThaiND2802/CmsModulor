namespace Commerce.Modules.Inventory.Application.DTOs.Responses;

public sealed record StockReservationDto(
    Guid Id,
    Guid OrderId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? ReleasedAtUtc,
    IReadOnlyList<StockReservationItemDto> Items);
