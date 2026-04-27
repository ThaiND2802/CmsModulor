using AutoMapper;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Contracts;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.OverrideSalePrice;

public sealed class OverrideSalePriceHandler : IRequestHandler<OverrideSalePriceCommand, ApiResponse<SaleDto>>
{
    private readonly SaleDbContext _dbContext;
    private readonly ISalePricingService _salePricingService;
    private readonly SaleSubmissionService _saleSubmissionService;
    private readonly IPermissionGate _permissionGate;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public OverrideSalePriceHandler(
        SaleDbContext dbContext,
        ISalePricingService salePricingService,
        SaleSubmissionService saleSubmissionService,
        IPermissionGate permissionGate,
        ICurrentUser currentUser,
        IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _salePricingService = salePricingService ?? throw new ArgumentNullException(nameof(salePricingService));
        _saleSubmissionService = saleSubmissionService ?? throw new ArgumentNullException(nameof(saleSubmissionService));
        _permissionGate = permissionGate ?? throw new ArgumentNullException(nameof(permissionGate));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(OverrideSalePriceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAppException("The current user is not authenticated.");
        }

        var allowed = await _permissionGate.HasPermissionAsync(
            _currentUser.UserId,
            SalePermissions.SalesOverridePrice,
            cancellationToken);

        if (!allowed)
        {
            throw new ForbiddenAppException($"Permission '{SalePermissions.SalesOverridePrice}' is required.");
        }

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
            throw new BusinessRuleAppException("Expired sales cannot be overridden.");
        }

        var item = sale.Items.FirstOrDefault(x => x.Id == command.ItemId)
            ?? throw new NotFoundAppException($"Sale item '{command.ItemId}' was not found.");

        item.OverridePrice = command.OverridePrice;
        item.OverrideReason = command.Reason.Trim();
        item.OverriddenBy = _currentUser.UserName ?? _currentUser.UserId;
        item.OverriddenAtUtc = DateTime.UtcNow;
        item.TotalAmount = (command.OverridePrice * item.Quantity) - item.DiscountAmount;

        var previousStatus = sale.Status;
        _salePricingService.Apply(sale);
        if (sale.TotalAmount < 0)
        {
            throw new ValidationAppException("Sale total amount cannot be negative.");
        }

        sale.Status = SaleStatus.Draft;
        _saleSubmissionService.AppendStatusHistory(
            sale,
            previousStatus,
            sale.Status,
            $"Price override applied to item '{item.Id}' by '{item.OverriddenBy}'. Reason: {item.OverrideReason}");

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(sale)
        };
    }
}
