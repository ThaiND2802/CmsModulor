using AutoMapper;
using Commerce.Modules.Order.Contracts;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Commerce.Modules.Sale.Application.Commands.SubmitSale;

public sealed class SubmitSaleHandler : IRequestHandler<SubmitSaleCommand, ApiResponse<SaleDto>>
{
    private readonly SaleSubmissionService _saleSubmissionService;
    private readonly IOrderModule _orderModule;
    private readonly IMapper _mapper;

    public SubmitSaleHandler(
        SaleSubmissionService saleSubmissionService,
        IOrderModule orderModule,
        IMapper mapper)
    {
        _saleSubmissionService = saleSubmissionService ?? throw new ArgumentNullException(nameof(saleSubmissionService));
        _orderModule = orderModule ?? throw new ArgumentNullException(nameof(orderModule));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(SubmitSaleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _saleSubmissionService.GetSaleForSubmissionAsync(command.SaleId, cancellationToken);
        SaleSubmissionService.EnsureCanSubmit(sale);

        await _saleSubmissionService.ReserveStockAsync(sale, cancellationToken);
        Guid? orderId = null;

        try
        {
            var orderResponse = await _orderModule.CreateOrderFromSaleAsync(
                SaleSubmissionService.MapToOrderRequest(sale),
                cancellationToken);

            if (!orderResponse.Success || !orderResponse.OrderId.HasValue)
            {
                throw new BusinessRuleAppException(orderResponse.ErrorMessage ?? "Failed to create order from sale.");
            }

            orderId = orderResponse.OrderId.Value;
            sale.Status = SaleStatus.Submitted;
            sale.OrderId = orderId.Value;
            sale.SubmittedAtUtc = DateTime.UtcNow;
            _saleSubmissionService.AppendStatusHistory(
                sale,
                SaleStatus.Priced,
                SaleStatus.Submitted,
                $"Order created from sale. OrderId: {orderId.Value}");
            await _saleSubmissionService.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            sale.Status = SaleStatus.Priced;
            sale.OrderId = null;
            sale.SubmittedAtUtc = null;
            if (sale.StatusHistory.LastOrDefault()?.ToStatus == SaleStatus.Submitted)
            {
                sale.StatusHistory.Remove(sale.StatusHistory.Last());
            }

            await _saleSubmissionService.ReleaseStockAsync(sale, cancellationToken);

            throw;
        }

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(sale)
        };
    }
}
