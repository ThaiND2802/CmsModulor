using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Application.Events;
using Commerce.Modules.Order.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrderAddressEntity = Commerce.Modules.Order.Domain.OrderAddress;
using OrderEntity = Commerce.Modules.Order.Domain.Order;
using OrderItemEntity = Commerce.Modules.Order.Domain.OrderItem;
using OrderStatusEntity = Commerce.Modules.Order.Domain.OrderStatus;
using OrderStatusHistoryEntity = Commerce.Modules.Order.Domain.OrderStatusHistory;

namespace Commerce.Modules.Order.Application.Commands.CreateOrder;

public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, ApiResponse<OrderDto>>
{
    private const string UniqueViolationSqlState = "23505";

    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateOrderHandler(OrderDbContext dbContext, IMapper mapper, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ApiResponse<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var utcNow = DateTime.UtcNow;
        var orderId = Guid.NewGuid();
        var orderNumber = $"ORD-{utcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var normalizedCurrency = command.Currency.Trim().ToUpperInvariant();
        var customerEmail = command.CustomerEmail.Trim();
        var customerPhone = string.IsNullOrWhiteSpace(command.CustomerPhone) ? null : command.CustomerPhone.Trim();
        var notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();

        var items = command.Items
            .Select(item =>
            {
                var totalAmount = (item.UnitPrice * item.Quantity) - item.DiscountAmount;

                return new OrderItemEntity
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName.Trim(),
                    ProductSku = item.ProductSku.Trim(),
                    VariantId = item.VariantId,
                    VariantName = string.IsNullOrWhiteSpace(item.VariantName) ? null : item.VariantName.Trim(),
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    DiscountAmount = item.DiscountAmount,
                    TotalAmount = totalAmount
                };
            })
            .ToList();

        var subtotalAmount = items.Sum(static item => item.TotalAmount);
        var totalAmount = subtotalAmount - command.DiscountAmount + command.ShippingAmount + command.TaxAmount;

        var order = new OrderEntity
        {
            Id = orderId,
            OrderNumber = orderNumber,
            CustomerId = command.CustomerId,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            Status = OrderStatusEntity.Pending,
            Notes = notes,
            ShippingAddress = MapAddress(command.ShippingAddress),
            BillingAddress = command.BillingAddress is null ? null : MapAddress(command.BillingAddress),
            SubtotalAmount = subtotalAmount,
            DiscountAmount = command.DiscountAmount,
            ShippingAmount = command.ShippingAmount,
            TaxAmount = command.TaxAmount,
            TotalAmount = totalAmount,
            Currency = normalizedCurrency,
            Items = items,
            StatusHistory =
            [
                new OrderStatusHistoryEntity
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    FromStatus = null,
                    ToStatus = OrderStatusEntity.Pending,
                    ChangedAtUtc = utcNow
                }
            ]
        };

        await _dbContext.Orders.AddAsync(order, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsOrderNumberConflict(exception))
        {
            throw new ConflictAppException("Order number already exists.");
        }

        await _mediator.Publish(new OrderCreated(order.Id, order.OrderNumber, order.CustomerId), cancellationToken);

        return new ApiResponse<OrderDto>
        {
            Status = StatusCodes.Status201Created,
            Data = _mapper.Map<OrderDto>(order)
        };
    }

    private static bool IsOrderNumberConflict(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: UniqueViolationSqlState } postgresException
        && string.Equals(postgresException.TableName, "order_orders", StringComparison.OrdinalIgnoreCase)
        && string.Equals(postgresException.ConstraintName, "IX_order_orders_order_number", StringComparison.OrdinalIgnoreCase);

    private static OrderAddressEntity MapAddress(CreateOrderAddressRequest request) =>
        new()
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = string.IsNullOrWhiteSpace(request.AddressLine2) ? null : request.AddressLine2.Trim(),
            City = request.City.Trim(),
            State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim(),
            Country = request.Country.Trim()
        };
}
