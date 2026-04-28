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

namespace Commerce.Modules.Sale.Application.Commands.ApplyCoupon;

public sealed class ApplyCouponHandler : IRequestHandler<ApplyCouponCommand, ApiResponse<SaleDto>>
{
    private readonly SaleDbContext _dbContext;
    private readonly ISaleCouponService _saleCouponService;
    private readonly ISalePricingService _salePricingService;
    private readonly IMapper _mapper;

    public ApplyCouponHandler(
        SaleDbContext dbContext,
        ISaleCouponService saleCouponService,
        ISalePricingService salePricingService,
        IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _saleCouponService = saleCouponService ?? throw new ArgumentNullException(nameof(saleCouponService));
        _salePricingService = salePricingService ?? throw new ArgumentNullException(nameof(salePricingService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(ApplyCouponCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _dbContext.Sales
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == command.SaleId, cancellationToken)
            ?? throw new NotFoundAppException($"Sale '{command.SaleId}' was not found.");

        if (!sale.IsMutable())
        {
            throw new BusinessRuleAppException($"Sale '{command.SaleId}' is no longer editable.");
        }

        if (sale.IsExpired(DateTime.UtcNow))
        {
            throw new BusinessRuleAppException("Expired sales cannot accept coupons.");
        }

        var coupon = await _saleCouponService.GetByCodeAsync(command.Code, cancellationToken);
        if (coupon is null)
        {
            throw new BusinessRuleAppException($"Coupon '{command.Code}' is invalid.");
        }

        if (coupon.ExpiresAtUtc.HasValue && coupon.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            throw new BusinessRuleAppException($"Coupon '{coupon.Code}' has expired.");
        }

        if (coupon.Type is not (SaleCouponType.FixedAmount or SaleCouponType.Percentage))
        {
            throw new BusinessRuleAppException($"Coupon '{coupon.Code}' is not supported in the MVP runtime.");
        }

        if (sale.Items.Count == 0)
        {
            throw new ValidationAppException("Sale must contain at least one item before applying a coupon.");
        }

        sale.CouponCode = coupon.Code;
        sale.CouponType = coupon.Type.ToString();
        sale.CouponValue = coupon.Value;
        _salePricingService.Apply(sale);

        if (sale.TotalAmount < 0)
        {
            throw new ValidationAppException("Sale total amount cannot be negative.");
        }

        sale.Status = SaleStatus.Draft;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(sale)
        };
    }
}
