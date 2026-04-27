namespace Commerce.Modules.Order.Contracts;

public sealed record CreateOrderFromSaleAddressRequest(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string? PostalCode,
    string Country);

public sealed record CreateOrderFromSaleItemRequest(
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid? VariantId,
    string? VariantName,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount);

public sealed record CreateOrderFromSaleRequest(
    Guid SaleId,
    Guid? CustomerId,
    string CustomerEmail,
    string? CustomerPhone,
    string? Notes,
    CreateOrderFromSaleAddressRequest ShippingAddress,
    CreateOrderFromSaleAddressRequest? BillingAddress,
    IReadOnlyList<CreateOrderFromSaleItemRequest> Items,
    decimal ShippingAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    string Currency);
