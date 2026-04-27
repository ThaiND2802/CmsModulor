using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Application.Events;
using Commerce.Modules.Order.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrderStatusEntity = Commerce.Modules.Order.Domain.OrderStatus;
using OrderStatusHistoryEntity = Commerce.Modules.Order.Domain.OrderStatusHistory;

namespace Commerce.Modules.Order.Application.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusHandler : IRequestHandler<ChangeOrderStatusCommand, ApiResponse<OrderDto>>
{
    private static readonly HashSet<(OrderStatusEntity From, OrderStatusEntity To)> AllowedTransitions =
    [
        (OrderStatusEntity.Confirmed, OrderStatusEntity.Processing),
        (OrderStatusEntity.Processing, OrderStatusEntity.Shipped),
        (OrderStatusEntity.Shipped, OrderStatusEntity.Delivered),
        (OrderStatusEntity.Shipped, OrderStatusEntity.Refunded),
        (OrderStatusEntity.Delivered, OrderStatusEntity.Refunded)
    ];

    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ChangeOrderStatusHandler(OrderDbContext dbContext, IMapper mapper, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ApiResponse<OrderDto>> Handle(ChangeOrderStatusCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var order = await _dbContext.Orders
            .Include(static x => x.Items)
            .Include(static x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Order '{command.Id}' was not found.");

        var fromStatus = order.Status;
        var toStatus = command.ToStatus;
        if (!AllowedTransitions.Contains((fromStatus, toStatus)))
        {
            throw new BusinessRuleAppException($"Cannot transition order from '{fromStatus}' to '{toStatus}'.");
        }

        order.Status = toStatus;
        var statusHistory = new OrderStatusHistoryEntity
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Order = order,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Note = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim(),
            ChangedAtUtc = DateTime.UtcNow
        };

        await _dbContext.OrderStatusHistory.AddAsync(statusHistory, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _mediator.Publish(
            new OrderStatusChanged(order.Id, order.OrderNumber, order.CustomerId, fromStatus, toStatus),
            cancellationToken);

        return new ApiResponse<OrderDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<OrderDto>(order)
        };
    }
}
