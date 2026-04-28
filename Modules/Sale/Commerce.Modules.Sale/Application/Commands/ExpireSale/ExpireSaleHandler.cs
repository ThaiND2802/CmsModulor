using AutoMapper;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Commerce.Modules.Sale.Application.Commands.ExpireSale;

public sealed class ExpireSaleHandler : IRequestHandler<ExpireSaleCommand, ApiResponse<SaleDto>>
{
    private readonly SaleSubmissionService _saleSubmissionService;
    private readonly IMapper _mapper;

    public ExpireSaleHandler(SaleSubmissionService saleSubmissionService, IMapper mapper)
    {
        _saleSubmissionService = saleSubmissionService ?? throw new ArgumentNullException(nameof(saleSubmissionService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<SaleDto>> Handle(ExpireSaleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _saleSubmissionService.GetSaleForSubmissionAsync(command.SaleId, cancellationToken);
        if (!SaleLifecycleTransitions.CanTransition(sale.Status, SaleStatus.Expired))
        {
            throw new BusinessRuleAppException($"Sale '{command.SaleId}' cannot be expired from status '{sale.Status}'.");
        }

        var previousStatus = sale.Status;
        sale.Status = SaleStatus.Expired;
        sale.ExpiresAtUtc ??= DateTime.UtcNow;
        _saleSubmissionService.AppendStatusHistory(
            sale,
            previousStatus,
            SaleStatus.Expired,
            string.IsNullOrWhiteSpace(command.Reason) ? "Sale expired." : command.Reason.Trim());

        await _saleSubmissionService.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SaleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<SaleDto>(sale)
        };
    }
}
