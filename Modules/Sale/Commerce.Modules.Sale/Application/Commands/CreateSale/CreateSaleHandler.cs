using AutoMapper;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Application.Commands.CreateSale;

public sealed class CreateSaleHandler : IRequestHandler<CreateSaleCommand, ApiResponse<SaleDto>>
{
    private readonly SaleDbContext _dbContext;
    private readonly ISalePricingService _salePricingService;
    private readonly IMapper _mapper;

    public CreateSaleHandler(SaleDbContext dbContext, ISalePricingService salePricingService, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _salePricingService = salePricingService ?? throw new ArgumentNullException(nameof(salePricingService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = new SaleEntity
        {
            Id = Guid.NewGuid(),
            SaleNumber = await GenerateSaleNumberAsync(cancellationToken),
            CustomerId = command.CustomerId,
            CustomerEmail = command.CustomerEmail.Trim(),
            CustomerPhone = command.CustomerPhone?.Trim(),
            Status = SaleStatus.Draft,
            ShippingAddress = MapAddress(command.ShippingAddress),
            BillingAddress = MapAddress(command.BillingAddress),
            ShippingAmount = command.ShippingAmount,
            BaseShippingAmount = command.ShippingAmount,
            DiscountAmount = command.DiscountAmount,
            BaseDiscountAmount = command.DiscountAmount,
            TaxAmount = command.TaxAmount,
            Currency = command.Currency.Trim().ToUpperInvariant(),
            Notes = command.Notes?.Trim(),
            ExpiresAtUtc = command.ExpiresAtUtc
        };

        foreach (var requestItem in command.Items)
        {
            var lineSubtotal = requestItem.UnitPrice * requestItem.Quantity;
            var lineTotal = lineSubtotal - requestItem.DiscountAmount;

            sale.Items.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = requestItem.ProductId,
                ProductName = requestItem.ProductName.Trim(),
                ProductSku = requestItem.ProductSku.Trim(),
                VariantId = requestItem.VariantId,
                VariantName = requestItem.VariantName?.Trim(),
                UnitPrice = requestItem.UnitPrice,
                Quantity = requestItem.Quantity,
                DiscountAmount = requestItem.DiscountAmount,
                TotalAmount = lineTotal
            });
        }

        _salePricingService.Apply(sale);
        sale.StatusHistory.Add(new SaleStatusHistory
        {
            Id = Guid.NewGuid(),
            FromStatus = null,
            ToStatus = SaleStatus.Draft,
            Note = "Sale draft created.",
            ChangedAtUtc = DateTime.UtcNow
        });

        _dbContext.Sales.Add(sale);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<SaleDto>(sale);
        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status201Created,
            Data = response
        };
    }

    private async Task<string> GenerateSaleNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"SAL-{today:yyyyMMdd}";
        var count = await _dbContext.Sales.CountAsync(x => x.SaleNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}-{count + 1:D4}";
    }

    private static SaleAddress? MapAddress(CreateSaleAddressRequest? request)
    {
        if (request is null)
        {
            return null;
        }

        return new SaleAddress
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            City = request.City.Trim(),
            State = request.State?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            Country = request.Country.Trim()
        };
    }
}
