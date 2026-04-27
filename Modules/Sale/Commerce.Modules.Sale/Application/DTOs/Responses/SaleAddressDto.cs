namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleAddressDto(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string? PostalCode,
    string Country);
