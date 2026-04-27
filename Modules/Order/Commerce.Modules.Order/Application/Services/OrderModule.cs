using Commerce.Modules.Order.Application.Commands.CreateOrder;
using Commerce.Modules.Order.Contracts;
using MediatR;

namespace Commerce.Modules.Order.Application.Services;

public sealed class OrderModule : IOrderModule
{
    private readonly IMediator _mediator;

    public OrderModule(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<OrderCreationResponse> CreateOrderFromSaleAsync(CreateOrderFromSaleRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _mediator.Send(
            new CreateOrderCommand
            {
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                Notes = request.Notes,
                ShippingAddress = new CreateOrderAddressRequest(
                    request.ShippingAddress.FullName,
                    request.ShippingAddress.PhoneNumber,
                    request.ShippingAddress.AddressLine1,
                    request.ShippingAddress.AddressLine2,
                    request.ShippingAddress.City,
                    request.ShippingAddress.State,
                    request.ShippingAddress.PostalCode,
                    request.ShippingAddress.Country),
                BillingAddress = request.BillingAddress is null
                    ? null
                    : new CreateOrderAddressRequest(
                        request.BillingAddress.FullName,
                        request.BillingAddress.PhoneNumber,
                        request.BillingAddress.AddressLine1,
                        request.BillingAddress.AddressLine2,
                        request.BillingAddress.City,
                        request.BillingAddress.State,
                        request.BillingAddress.PostalCode,
                        request.BillingAddress.Country),
                Items = request.Items
                    .Select(static item => new CreateOrderItemRequest(
                        item.ProductId,
                        item.ProductName,
                        item.ProductSku,
                        item.VariantId,
                        item.VariantName,
                        item.UnitPrice,
                        item.Quantity,
                        item.DiscountAmount))
                    .ToArray(),
                ShippingAmount = request.ShippingAmount,
                DiscountAmount = request.DiscountAmount,
                TaxAmount = request.TaxAmount,
                Currency = request.Currency
            },
            cancellationToken);

        return new OrderCreationResponse(
            Success: response.Data is not null,
            OrderId: response.Data?.Id,
            OrderNumber: response.Data?.OrderNumber,
            ErrorCode: response.Data is null ? response.Status.ToString() : null,
            ErrorMessage: response.Data is null ? "Order creation returned no data." : null);
    }
}
