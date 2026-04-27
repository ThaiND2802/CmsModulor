using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Commands.CreateOrder;

public sealed record CreateOrderAddressRequest(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string? PostalCode,
    string Country);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid? VariantId,
    string? VariantName,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount = 0);

public sealed record CreateOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid? CustomerId { get; init; }

    public string CustomerEmail { get; init; } = default!;

    public string? CustomerPhone { get; init; }

    public string? Notes { get; init; }

    public CreateOrderAddressRequest ShippingAddress { get; init; } = default!;

    public CreateOrderAddressRequest? BillingAddress { get; init; }

    public IReadOnlyList<CreateOrderItemRequest> Items { get; init; } = [];

    public decimal ShippingAmount { get; init; }

    public decimal DiscountAmount { get; init; }

    public decimal TaxAmount { get; init; }

    public string Currency { get; init; } = "VND";
}
