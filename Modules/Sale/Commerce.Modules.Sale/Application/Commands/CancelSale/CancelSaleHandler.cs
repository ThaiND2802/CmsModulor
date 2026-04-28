using AutoMapper;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using CommerceCore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Commerce.Modules.Sale.Application.Commands.CancelSale;

public sealed class CancelSaleHandler : IRequestHandler<CancelSaleCommand, ApiResponse<SaleDto>>
{
    private readonly SaleSubmissionService _saleSubmissionService;
    private readonly IMapper _mapper;

    public CancelSaleHandler(SaleSubmissionService saleSubmissionService, IMapper mapper)
    {
        _saleSubmissionService = saleSubmissionService ?? throw new ArgumentNullException(nameof(saleSubmissionService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _saleSubmissionService.GetSaleForSubmissionAsync(command.SaleId, cancellationToken);
        SaleSubmissionService.EnsureCanCancel(sale);

        var previousStatus = sale.Status;
        SaleLifecycleTransitions.EnsureCanTransition(previousStatus, SaleStatus.Cancelled, "cancel");
        sale.Status = SaleStatus.Cancelled;
        _saleSubmissionService.AppendStatusHistory(
            sale,
            previousStatus,
            SaleStatus.Cancelled,
            string.IsNullOrWhiteSpace(command.Reason) ? "Sale cancelled." : command.Reason.Trim());

        await _saleSubmissionService.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(sale)
        };
    }
}
