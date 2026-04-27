using AutoMapper;
using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Inventory.Application.DTOs.Responses;
using Commerce.Modules.Inventory.Application.Events;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Inventory.Application.Commands.ReserveStock;

public sealed class ReserveStockHandler : IRequestHandler<ReserveStockCommand, ApiResponse<StockReservationDto>>
{
    private readonly InventoryDbContext _inventoryDbContext;
    private readonly ICatalogVariantLookup _catalogVariantLookup;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ReserveStockHandler(
        InventoryDbContext inventoryDbContext,
        ICatalogVariantLookup catalogVariantLookup,
        IMapper mapper,
        IMediator mediator)
    {
        _inventoryDbContext = inventoryDbContext ?? throw new ArgumentNullException(nameof(inventoryDbContext));
        _catalogVariantLookup = catalogVariantLookup ?? throw new ArgumentNullException(nameof(catalogVariantLookup));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ApiResponse<StockReservationDto>> Handle(ReserveStockCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existingReservation = await _inventoryDbContext.StockReservations
            .Include(x => x.Items)
                .ThenInclude(x => x.InventoryItem)
            .FirstOrDefaultAsync(x => x.OrderId == command.OrderId, cancellationToken);

        if (existingReservation is not null)
        {
            if (HasSameItems(existingReservation, command))
            {
                return new ApiResponse<StockReservationDto>
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<StockReservationDto>(existingReservation)
                };
            }

            throw new ConflictAppException($"A reservation already exists for order '{command.OrderId}'.");
        }

        var requestedVariantIds = command.Items
            .Select(static x => x.VariantId)
            .ToArray();

        foreach (var requestedVariantId in requestedVariantIds)
        {
            var variant = await _catalogVariantLookup.GetByIdAsync(requestedVariantId, cancellationToken);
            if (variant is null)
            {
                throw new NotFoundAppException($"Product variant '{requestedVariantId}' was not found.");
            }

            if (!variant.IsActive)
            {
                throw new ValidationAppException($"Product variant '{requestedVariantId}' is inactive.");
            }
        }

        var inventoryItems = await _inventoryDbContext.InventoryItems
            .Where(x => requestedVariantIds.Contains(x.VariantId))
            .ToListAsync(cancellationToken);

        if (inventoryItems.Count != requestedVariantIds.Length)
        {
            throw new ValidationAppException("One or more inventory items were not found for the requested variants.");
        }

        var inventoryItemsByVariantId = inventoryItems.ToDictionary(x => x.VariantId);
        foreach (var requestedItem in command.Items)
        {
            var inventoryItem = inventoryItemsByVariantId[requestedItem.VariantId];
            if (inventoryItem.AvailableQuantity < requestedItem.Quantity)
            {
                throw new ConflictAppException($"Insufficient available stock for variant '{requestedItem.VariantId}'.");
            }
        }

        var reservation = StockReservation.Create(command.OrderId);
        var reservedItems = new List<StockReservedItem>(command.Items.Count);

        foreach (var requestedItem in command.Items)
        {
            var inventoryItem = inventoryItemsByVariantId[requestedItem.VariantId];
            inventoryItem.Reserve(requestedItem.Quantity);

            var reservationItem = StockReservationItem.Create(reservation.Id, inventoryItem.Id, requestedItem.Quantity);
            reservation.Items.Add(reservationItem);

            var movement = StockMovement.Create(
                inventoryItem.Id,
                StockMovementType.Reserved,
                requestedItem.Quantity,
                inventoryItem.OnHandQuantity,
                inventoryItem.ReservedQuantity,
                "Stock reserved",
                "order",
                command.OrderId);

            await _inventoryDbContext.StockMovements.AddAsync(movement, cancellationToken);

            reservedItems.Add(new StockReservedItem(
                inventoryItem.Id,
                inventoryItem.VariantId,
                inventoryItem.Sku,
                requestedItem.Quantity));
        }

        await _inventoryDbContext.StockReservations.AddAsync(reservation, cancellationToken);

        StockReservation createdReservation;
        try
        {
            await _inventoryDbContext.SaveChangesAsync(cancellationToken);

            createdReservation = await _inventoryDbContext.StockReservations
                .Include(x => x.Items)
                    .ThenInclude(x => x.InventoryItem)
                .FirstAsync(x => x.Id == reservation.Id, cancellationToken);
        }
        catch (DbUpdateException)
        {
            var persistedReservation = await _inventoryDbContext.StockReservations
                .AsNoTracking()
                .Include(x => x.Items)
                    .ThenInclude(x => x.InventoryItem)
                .FirstOrDefaultAsync(x => x.OrderId == command.OrderId, cancellationToken);

            if (persistedReservation is not null && HasSameItems(persistedReservation, command))
            {
                return new ApiResponse<StockReservationDto>
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<StockReservationDto>(persistedReservation)
                };
            }

            throw new ConflictAppException($"A reservation already exists for order '{command.OrderId}'.");
        }

        await _mediator.Publish(new StockReserved(createdReservation.Id, createdReservation.OrderId, reservedItems), cancellationToken);

        return new ApiResponse<StockReservationDto>
        {
            Status = StatusCodes.Status201Created,
            Data = _mapper.Map<StockReservationDto>(createdReservation)
        };
    }

    private static bool HasSameItems(StockReservation reservation, ReserveStockCommand command)
    {
        if (reservation.Status != StockReservationStatus.Active || reservation.Items.Count != command.Items.Count)
        {
            return false;
        }

        var existingItems = reservation.Items
            .Select(static item => new { item.InventoryItem.VariantId, item.Quantity })
            .OrderBy(static item => item.VariantId)
            .ToArray();

        var requestedItems = command.Items
            .Select(static item => new { item.VariantId, item.Quantity })
            .OrderBy(static item => item.VariantId)
            .ToArray();

        return existingItems.SequenceEqual(requestedItems);
    }
}
