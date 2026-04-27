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

namespace Commerce.Modules.Inventory.Application.Commands.AdjustStock;

public sealed class AdjustStockHandler : IRequestHandler<AdjustStockCommand, ApiResponse<InventoryStockDto>>
{
    private readonly InventoryDbContext _inventoryDbContext;
    private readonly ICatalogVariantLookup _catalogVariantLookup;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public AdjustStockHandler(
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

    public async Task<ApiResponse<InventoryStockDto>> Handle(AdjustStockCommand command, CancellationToken cancellationToken)
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

        var inventoryItem = await _inventoryDbContext.InventoryItems
            .FirstOrDefaultAsync(x => x.VariantId == command.VariantId, cancellationToken);

        if (inventoryItem is null)
        {
            throw new NotFoundAppException($"Inventory item for variant '{command.VariantId}' was not found.");
        }

        var normalizedReason = command.Reason.Trim();
        var normalizedReferenceType = string.IsNullOrWhiteSpace(command.ReferenceType)
            ? null
            : command.ReferenceType.Trim();

        try
        {
            inventoryItem.Adjust(command.QuantityDelta);
        }
        catch (InvalidOperationException exception)
        {
            throw new BusinessRuleAppException(exception.Message);
        }

        var stockMovement = StockMovement.Create(
            inventoryItem.Id,
            StockMovementType.Adjusted,
            command.QuantityDelta,
            inventoryItem.OnHandQuantity,
            inventoryItem.ReservedQuantity,
            normalizedReason,
            normalizedReferenceType,
            command.ReferenceId);

        await _inventoryDbContext.StockMovements.AddAsync(stockMovement, cancellationToken);
        await _inventoryDbContext.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new StockAdjusted(
                inventoryItem.Id,
                inventoryItem.VariantId,
                inventoryItem.Sku,
                command.QuantityDelta,
                inventoryItem.OnHandQuantity,
                inventoryItem.ReservedQuantity),
            cancellationToken);

        return new ApiResponse<InventoryStockDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<InventoryStockDto>(inventoryItem)
        };
    }
}
