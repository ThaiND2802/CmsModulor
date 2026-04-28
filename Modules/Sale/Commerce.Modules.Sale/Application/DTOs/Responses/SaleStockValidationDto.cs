namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleStockValidationDto(
    Guid SaleId,
    bool IsAvailable,
    bool Guaranteed,
    string Mode,
    string? ReservationReference,
    IReadOnlyList<SaleStockValidationItemDto> Items);
