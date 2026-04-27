using AutoMapper;
using Commerce.Modules.Inventory.Application.DTOs.Responses;
using Commerce.Modules.Inventory.Application.Events;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Inventory.Application.Commands.ReleaseStock;

public sealed class ReleaseStockHandler : IRequestHandler<ReleaseStockCommand, ApiResponse<StockReservationDto>>
{
    private readonly InventoryDbContext _inventoryDbContext;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ReleaseStockHandler(
        InventoryDbContext inventoryDbContext,
        IMapper mapper,
        IMediator mediator)
    {
        _inventoryDbContext = inventoryDbContext ?? throw new ArgumentNullException(nameof(inventoryDbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ApiResponse<StockReservationDto>> Handle(ReleaseStockCommand command, CancellationToken cancellationToken)
    {
        var reservation = await _inventoryDbContext.StockReservations
            .Include(x => x.Items)
                .ThenInclude(x => x.InventoryItem)
            .FirstOrDefaultAsync(x => x.OrderId == command.OrderId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundAppException($"Reservation for order '{command.OrderId}' was not found.");
        }

        if (reservation.Status == StockReservationStatus.Released)
        {
            return new ApiResponse<StockReservationDto>
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<StockReservationDto>(reservation)
            };
        }

        var releasedItems = new List<StockReleasedItem>(reservation.Items.Count);
        foreach (var reservationItem in reservation.Items)
        {
            reservationItem.InventoryItem.Release(reservationItem.Quantity);

            var movement = StockMovement.Create(
                reservationItem.InventoryItemId,
                StockMovementType.Released,
                -reservationItem.Quantity,
                reservationItem.InventoryItem.OnHandQuantity,
                reservationItem.InventoryItem.ReservedQuantity,
                "Stock released",
                "order",
                command.OrderId);

            await _inventoryDbContext.StockMovements.AddAsync(movement, cancellationToken);

            releasedItems.Add(new StockReleasedItem(
                reservationItem.InventoryItemId,
                reservationItem.InventoryItem.VariantId,
                reservationItem.InventoryItem.Sku,
                reservationItem.Quantity));
        }

        reservation.Release(DateTime.UtcNow);
        await _inventoryDbContext.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new StockReleased(reservation.Id, reservation.OrderId, releasedItems), cancellationToken);

        return new ApiResponse<StockReservationDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<StockReservationDto>(reservation)
        };
    }
}
