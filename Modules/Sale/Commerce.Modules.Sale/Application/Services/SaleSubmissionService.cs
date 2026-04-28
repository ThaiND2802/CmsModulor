using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Order.Contracts;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Application.Services;

public sealed class SaleSubmissionService
{
    private readonly SaleDbContext _dbContext;
    private readonly IInventoryModule _inventoryModule;

    public SaleSubmissionService(SaleDbContext dbContext, IInventoryModule inventoryModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _inventoryModule = inventoryModule ?? throw new ArgumentNullException(nameof(inventoryModule));
    }

    public async Task<SaleEntity> GetSaleForSubmissionAsync(Guid saleId, CancellationToken cancellationToken)
    {
        return await _dbContext.Sales
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == saleId, cancellationToken)
            ?? throw new NotFoundAppException($"Sale '{saleId}' was not found.");
    }

    public static void EnsureCanValidateStock(SaleEntity sale)
    {
        ArgumentNullException.ThrowIfNull(sale);

        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new BusinessRuleAppException("Cancelled sales cannot validate stock.");
        }

        if (sale.Status == SaleStatus.Submitted)
        {
            throw new BusinessRuleAppException("Submitted sales cannot validate stock.");
        }

        if (sale.Status == SaleStatus.Expired || sale.IsExpired(DateTime.UtcNow))
        {
            throw new BusinessRuleAppException("Expired sales cannot validate stock.");
        }

        if (sale.Items.Count == 0)
        {
            throw new ValidationAppException("Sale must contain at least one item.");
        }
    }

    public static void EnsureCanSubmit(SaleEntity sale)
    {
        ArgumentNullException.ThrowIfNull(sale);

        if (sale.Status == SaleStatus.Submitted)
        {
            return;
        }

        if (sale.Status == SaleStatus.Expired || sale.IsExpired(DateTime.UtcNow))
        {
            throw new BusinessRuleAppException("Expired sales cannot be submitted.");
        }

        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new BusinessRuleAppException("Cancelled sales cannot be submitted.");
        }

        if (sale.Status != SaleStatus.Priced || sale.OrderId.HasValue)
        {
            throw new BusinessRuleAppException("Only priced sales can be submitted unless already submitted.");
        }

        if (sale.Items.Count == 0)
        {
            throw new ValidationAppException("Sale must contain at least one item.");
        }

        if (string.IsNullOrWhiteSpace(sale.CustomerEmail))
        {
            throw new ValidationAppException("Sale customer email is required.");
        }

        if (sale.ShippingAddress is null)
        {
            throw new ValidationAppException("Sale shipping address is required.");
        }

        if (sale.Items.Any(static item => !item.ProductId.HasValue))
        {
            throw new ValidationAppException("Sale items must contain a product snapshot before submission.");
        }
    }

    public static void EnsureCanCancel(SaleEntity sale)
    {
        ArgumentNullException.ThrowIfNull(sale);

        if (!SaleLifecycleTransitions.CanTransition(sale.Status, SaleStatus.Cancelled))
        {
            throw new BusinessRuleAppException("Only draft or priced sales can be cancelled.");
        }
    }

    public async Task<SaleStockValidationDto> ReserveStockAsync(SaleEntity sale, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sale);

        var reservationItems = sale.Items
            .Where(static item => item.VariantId.HasValue)
            .Select(static item => new ReserveStockItemRequest(item.VariantId!.Value, item.Quantity))
            .ToArray();

        if (reservationItems.Length == 0)
        {
            return new SaleStockValidationDto(
                sale.Id,
                true,
                false,
                "preview_only",
                null,
                Array.Empty<SaleStockValidationItemDto>());
        }

        var response = await _inventoryModule.ReserveStockAsync(
            new ReserveStockRequest(sale.Id, reservationItems),
            cancellationToken);

        return new SaleStockValidationDto(
            sale.Id,
            true,
            false,
            "preview_only",
            response?.Id.ToString(),
            reservationItems
                .Select(static item => new SaleStockValidationItemDto(item.VariantId, item.Quantity, true, null))
                .ToArray());
    }

    public async Task ReleaseStockAsync(SaleEntity sale, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sale);

        var hasReservedVariantItems = sale.Items.Any(static item => item.VariantId.HasValue);
        if (!hasReservedVariantItems)
        {
            return;
        }

        await _inventoryModule.ReleaseStockAsync(new ReleaseStockRequest(sale.Id), cancellationToken);
    }

    public static CreateOrderFromSaleRequest MapToOrderRequest(SaleEntity sale)
    {
        ArgumentNullException.ThrowIfNull(sale);
        ArgumentNullException.ThrowIfNull(sale.ShippingAddress);

        return new CreateOrderFromSaleRequest(
            sale.Id,
            sale.CustomerId,
            sale.CustomerEmail,
            sale.CustomerPhone,
            sale.Notes,
            new CreateOrderFromSaleAddressRequest(
                sale.ShippingAddress.FullName,
                sale.ShippingAddress.PhoneNumber,
                sale.ShippingAddress.AddressLine1,
                sale.ShippingAddress.AddressLine2,
                sale.ShippingAddress.City,
                sale.ShippingAddress.State,
                sale.ShippingAddress.PostalCode,
                sale.ShippingAddress.Country),
            sale.BillingAddress is null
                ? null
                : new CreateOrderFromSaleAddressRequest(
                    sale.BillingAddress.FullName,
                    sale.BillingAddress.PhoneNumber,
                    sale.BillingAddress.AddressLine1,
                    sale.BillingAddress.AddressLine2,
                    sale.BillingAddress.City,
                    sale.BillingAddress.State,
                    sale.BillingAddress.PostalCode,
                    sale.BillingAddress.Country),
            sale.Items
                .Select(static item => new CreateOrderFromSaleItemRequest(
                    item.ProductId!.Value,
                    item.ProductName,
                    item.ProductSku,
                    item.VariantId,
                    item.VariantName,
                    item.UnitPrice,
                    item.Quantity,
                    item.DiscountAmount))
                .ToArray(),
            sale.ShippingAmount,
            sale.DiscountAmount,
            sale.TaxAmount,
            sale.Currency);
    }

    public void AppendStatusHistory(SaleEntity sale, SaleStatus? fromStatus, SaleStatus toStatus, string? note)
    {
        ArgumentNullException.ThrowIfNull(sale);

        var history = new SaleStatusHistory
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            Sale = sale,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Note = note,
            ChangedAtUtc = DateTime.UtcNow
        };

        sale.StatusHistory.Add(history);
        _dbContext.SaleStatusHistory.Add(history);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
