using Commerce.Modules.Order.Domain;
using MediatR;

namespace Commerce.Modules.Order.Application.Events;

public sealed record OrderCancelled(
    Guid OrderId,
    string OrderNumber,
    Guid? CustomerId,
    OrderStatus FromStatus,
    string CancelReason) : INotification;
