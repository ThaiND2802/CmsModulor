using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.CreateSale;

public sealed record CreateSaleAddressRequest(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string? PostalCode,
    string Country);

public sealed record CreateSaleItemRequest(
    Guid? ProductId,
    string ProductName,
    string ProductSku,
    Guid? VariantId,
    string? VariantName,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount = 0);

public sealed record CreateSaleCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid? CustomerId { get; init; }

    public string CustomerEmail { get; init; } = default!;

    public string? CustomerPhone { get; init; }

    public string? Notes { get; init; }

    public CreateSaleAddressRequest? ShippingAddress { get; init; }

    public CreateSaleAddressRequest? BillingAddress { get; init; }

    public IReadOnlyList<CreateSaleItemRequest> Items { get; init; } = [];

    public decimal ShippingAmount { get; init; }

    public decimal DiscountAmount { get; init; }

    public decimal TaxAmount { get; init; }

    public DateTime? ExpiresAtUtc { get; init; }

    public string Currency { get; init; } = default!;
}
