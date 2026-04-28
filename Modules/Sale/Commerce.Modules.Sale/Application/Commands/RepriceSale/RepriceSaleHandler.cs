using AutoMapper;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.RepriceSale;

public sealed class RepriceSaleHandler : IRequestHandler<RepriceSaleCommand, ApiResponse<SaleDto>>
{
    private readonly SaleDbContext _dbContext;
    private readonly ISalePricingService _salePricingService;
    private readonly IMapper _mapper;

    public RepriceSaleHandler(
        SaleDbContext dbContext,
        ISalePricingService salePricingService,
        IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _salePricingService = salePricingService ?? throw new ArgumentNullException(nameof(salePricingService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(RepriceSaleCommand command, CancellationToken cancellationToken)
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

        sale.BaseDiscountAmount = command.DiscountAmount;
        sale.BaseShippingAmount = command.ShippingAmount;
        sale.TaxAmount = command.TaxAmount;

        _salePricingService.Apply(sale);

        if (sale.TotalAmount < 0)
        {
            throw new ValidationAppException("Sale total amount cannot be negative.");
        }

        if (sale.IsExpired(DateTime.UtcNow))
        {
            SaleLifecycleTransitions.EnsureCanTransition(sale.Status, Domain.SaleStatus.Expired, "expire");
            sale.Status = Domain.SaleStatus.Expired;
        }
        else
        {
            var targetStatus = sale.Items.Count > 0 ? Domain.SaleStatus.Priced : Domain.SaleStatus.Draft;
            SaleLifecycleTransitions.EnsureCanTransition(sale.Status, targetStatus, "reprice");
            sale.Status = targetStatus;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(sale)
        };
    }
}
