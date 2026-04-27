using MediatR;

namespace Commerce.Modules.Order.Application.Events;

public sealed record OrderCreated(
    Guid OrderId,
    string OrderNumber,
    Guid? CustomerId) : INotification;
