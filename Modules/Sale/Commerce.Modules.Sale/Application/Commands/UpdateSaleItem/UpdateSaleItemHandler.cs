using AutoMapper;
using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleItem;

public sealed class UpdateSaleItemHandler : IRequestHandler<UpdateSaleItemCommand, ApiResponse<SaleDto>>
{
    private readonly SaleDbContext _dbContext;
    private readonly ICatalogVariantLookup _catalogVariantLookup;
    private readonly ISalePricingService _salePricingService;
    private readonly IMapper _mapper;

    public UpdateSaleItemHandler(
        SaleDbContext dbContext,
        ICatalogVariantLookup catalogVariantLookup,
        ISalePricingService salePricingService,
        IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _catalogVariantLookup = catalogVariantLookup ?? throw new ArgumentNullException(nameof(catalogVariantLookup));
        _salePricingService = salePricingService ?? throw new ArgumentNullException(nameof(salePricingService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(UpdateSaleItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _dbContext.Sales
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == command.SaleId, cancellationToken)
            ?? throw new NotFoundAppException($"Sale '{command.SaleId}' was not found.");

        if (!sale.IsMutable())
        {
            throw new ValidationAppException($"Sale '{command.SaleId}' is no longer editable.");
        }

        var item = sale.Items.FirstOrDefault(x => x.Id == command.ItemId)
            ?? throw new NotFoundAppException($"Sale item '{command.ItemId}' was not found.");

        var variant = await _catalogVariantLookup.GetByIdAsync(command.VariantId, cancellationToken);
        if (variant is null)
        {
            throw new NotFoundAppException($"Product variant '{command.VariantId}' was not found.");
        }

        if (!variant.IsActive)
        {
            throw new ValidationAppException($"Product variant '{command.VariantId}' is inactive.");
        }

        var lineSubtotal = variant.UnitPrice * command.Quantity;
        if (command.DiscountAmount > lineSubtotal)
        {
            throw new ValidationAppException("Item discount must be less than or equal to the line subtotal.");
        }

        item.ProductId = variant.ProductId;
        item.ProductName = variant.ProductName.Trim();
        item.ProductSku = variant.Sku.Trim();
        item.VariantId = variant.Id;
        item.VariantName = string.IsNullOrWhiteSpace(variant.VariantName) ? null : variant.VariantName.Trim();
        item.UnitPrice = variant.UnitPrice;
        item.Quantity = command.Quantity;
        item.DiscountAmount = command.DiscountAmount;
        item.TotalAmount = lineSubtotal - command.DiscountAmount;

        _salePricingService.Apply(sale);
        sale.Status = Domain.SaleStatus.Draft;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(sale)
        };
    }
}
