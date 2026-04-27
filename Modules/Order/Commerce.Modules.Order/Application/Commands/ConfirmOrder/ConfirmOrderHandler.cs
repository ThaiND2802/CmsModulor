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

namespace Commerce.Modules.Order.Application.Commands.ConfirmOrder;

public sealed class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand, ApiResponse<OrderDto>>
{
    private readonly OrderDbContext _dbContext;
    private readonly IInventoryModule _inventoryModule;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ConfirmOrderHandler(OrderDbContext dbContext, IInventoryModule inventoryModule, IMapper mapper, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _inventoryModule = inventoryModule ?? throw new ArgumentNullException(nameof(inventoryModule));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ApiResponse<OrderDto>> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var order = await _dbContext.Orders
            .Include(static x => x.Items)
            .Include(static x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Order '{command.Id}' was not found.");

        if (order.Status != OrderStatusEntity.Pending)
        {
            throw new BusinessRuleAppException("Only pending orders can be confirmed.");
        }

        var reservationItems = order.Items
            .Where(static item => item.VariantId.HasValue)
            .Select(static item => new ReserveStockItemRequest(item.VariantId!.Value, item.Quantity))
            .ToArray();

        if (reservationItems.Length > 0)
        {
            await _inventoryModule.ReserveStockAsync(new ReserveStockRequest(order.Id, reservationItems), cancellationToken);
        }

        var previousStatus = order.Status;
        order.Status = OrderStatusEntity.Confirmed;
        var statusHistory = new OrderStatusHistoryEntity
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Order = order,
            FromStatus = previousStatus,
            ToStatus = OrderStatusEntity.Confirmed,
            Note = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim(),
            ChangedAtUtc = DateTime.UtcNow
        };

        await _dbContext.OrderStatusHistory.AddAsync(statusHistory, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _mediator.Publish(
            new OrderStatusChanged(order.Id, order.OrderNumber, order.CustomerId, previousStatus, OrderStatusEntity.Confirmed),
            cancellationToken);

        return new ApiResponse<OrderDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<OrderDto>(order)
        };
    }
}
