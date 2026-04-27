using AutoMapper;
using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.AddSaleItem;

public sealed class AddSaleItemHandler : IRequestHandler<AddSaleItemCommand, ApiResponse<SaleDto>>
{
    private readonly SaleDbContext _dbContext;
    private readonly ICatalogVariantLookup _catalogVariantLookup;
    private readonly ISalePricingService _salePricingService;
    private readonly IMapper _mapper;

    public AddSaleItemHandler(
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

    public async Task<ApiResponse<SaleDto>> Handle(AddSaleItemCommand command, CancellationToken cancellationToken)
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

        var newItem = new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = variant.ProductId,
            ProductName = variant.ProductName.Trim(),
            ProductSku = variant.Sku.Trim(),
            VariantId = variant.Id,
            VariantName = string.IsNullOrWhiteSpace(variant.VariantName) ? null : variant.VariantName.Trim(),
            UnitPrice = variant.UnitPrice,
            Quantity = command.Quantity,
            DiscountAmount = command.DiscountAmount,
            TotalAmount = lineSubtotal - command.DiscountAmount
        };

        await _dbContext.SaleItems.AddAsync(newItem, cancellationToken);

        _salePricingService.Apply(sale);
        sale.Status = Domain.SaleStatus.Draft;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var responseSale = await _dbContext.Sales
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .FirstAsync(x => x.Id == command.SaleId, cancellationToken);

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(responseSale)
        };
    }
}
