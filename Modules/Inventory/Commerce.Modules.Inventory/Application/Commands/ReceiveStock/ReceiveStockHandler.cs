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

namespace Commerce.Modules.Inventory.Application.Commands.ReceiveStock;

public sealed class ReceiveStockHandler : IRequestHandler<ReceiveStockCommand, ApiResponse<InventoryStockDto>>
{
    private readonly InventoryDbContext _inventoryDbContext;
    private readonly ICatalogVariantLookup _catalogVariantLookup;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ReceiveStockHandler(
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

    public async Task<ApiResponse<InventoryStockDto>> Handle(ReceiveStockCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var variant = await _catalogVariantLookup.GetByIdAsync(command.VariantId, cancellationToken);

        if (variant is null)
        {
            throw new NotFoundAppException($"Product variant '{command.VariantId}' was not found.");
        }

        if (!variant.IsActive)
        {
            throw new ValidationAppException($"Product variant '{command.VariantId}' is inactive.");
        }

        var normalizedSku = variant.Sku;
        var normalizedReason = command.Reason.Trim();
        var normalizedReferenceType = string.IsNullOrWhiteSpace(command.ReferenceType)
            ? null
            : command.ReferenceType.Trim();

        var inventoryItem = await _inventoryDbContext.InventoryItems
            .FirstOrDefaultAsync(x => x.VariantId == command.VariantId, cancellationToken);

        if (inventoryItem is null)
        {
            inventoryItem = InventoryItem.Create(command.VariantId, normalizedSku);
            await _inventoryDbContext.InventoryItems.AddAsync(inventoryItem, cancellationToken);
        }

        inventoryItem.Receive(command.Quantity);

        var stockMovement = StockMovement.Create(
            inventoryItem.Id,
            StockMovementType.Received,
            command.Quantity,
            inventoryItem.OnHandQuantity,
            inventoryItem.ReservedQuantity,
            normalizedReason,
            normalizedReferenceType,
            command.ReferenceId);

        await _inventoryDbContext.StockMovements.AddAsync(stockMovement, cancellationToken);
        await _inventoryDbContext.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new StockReceived(
                inventoryItem.Id,
                inventoryItem.VariantId,
                inventoryItem.Sku,
                command.Quantity,
                inventoryItem.OnHandQuantity,
                inventoryItem.ReservedQuantity),
            cancellationToken);

        return new ApiResponse<InventoryStockDto>
        {
            Status = StatusCodes.Status201Created,
            Data = _mapper.Map<InventoryStockDto>(inventoryItem)
        };
    }
}
