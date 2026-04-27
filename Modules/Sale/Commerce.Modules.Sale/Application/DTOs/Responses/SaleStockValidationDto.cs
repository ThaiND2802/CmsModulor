namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleStockValidationDto(
    Guid SaleId,
    bool IsAvailable,
    string? ReservationReference,
    IReadOnlyList<SaleStockValidationItemDto> Items);
