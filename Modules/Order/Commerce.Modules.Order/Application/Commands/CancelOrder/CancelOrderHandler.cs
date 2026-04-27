using AutoMapper;
using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Contracts.Requests;
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

namespace Commerce.Modules.Order.Application.Commands.CancelOrder;

public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand, ApiResponse<OrderDto>>
{
    private readonly OrderDbContext _dbContext;
    private readonly IInventoryModule _inventoryModule;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CancelOrderHandler(OrderDbContext dbContext, IInventoryModule inventoryModule, IMapper mapper, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _inventoryModule = inventoryModule ?? throw new ArgumentNullException(nameof(inventoryModule));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ApiResponse<OrderDto>> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var order = await _dbContext.Orders
            .Include(static x => x.Items)
            .Include(static x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Order '{command.Id}' was not found.");

        if (order.Status == OrderStatusEntity.Cancelled)
        {
            return new ApiResponse<OrderDto>
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<OrderDto>(order)
            };
        }

        if (order.Status is not (OrderStatusEntity.Pending or OrderStatusEntity.Confirmed or OrderStatusEntity.Processing))
        {
            throw new BusinessRuleAppException("Only pending, confirmed, or processing orders can be cancelled.");
        }

        if (order.Status is OrderStatusEntity.Confirmed or OrderStatusEntity.Processing)
        {
            await _inventoryModule.ReleaseStockAsync(new ReleaseStockRequest(order.Id), cancellationToken);
        }

        var previousStatus = order.Status;
        var cancelReason = command.CancelReason.Trim();
        order.Status = OrderStatusEntity.Cancelled;
        order.CancelReason = cancelReason;
        var statusHistory = new OrderStatusHistoryEntity
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Order = order,
            FromStatus = previousStatus,
            ToStatus = OrderStatusEntity.Cancelled,
            Note = cancelReason,
            ChangedAtUtc = DateTime.UtcNow
        };

        await _dbContext.OrderStatusHistory.AddAsync(statusHistory, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _mediator.Publish(
            new OrderCancelled(order.Id, order.OrderNumber, order.CustomerId, previousStatus, cancelReason),
            cancellationToken);
        await _mediator.Publish(
            new OrderStatusChanged(order.Id, order.OrderNumber, order.CustomerId, previousStatus, OrderStatusEntity.Cancelled),
            cancellationToken);

        return new ApiResponse<OrderDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<OrderDto>(order)
        };
    }
}
