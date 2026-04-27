namespace Commerce.Modules.Order.Application.DTOs.Responses;

public sealed record OrderAddressDto(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string? PostalCode,
    string Country);
