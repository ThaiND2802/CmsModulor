using Commerce.Modules.Order.Domain;
using MediatR;

namespace Commerce.Modules.Order.Application.Events;

public sealed record OrderStatusChanged(
    Guid OrderId,
    string OrderNumber,
    Guid? CustomerId,
    OrderStatus FromStatus,
    OrderStatus ToStatus) : INotification;
