using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Services;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Commerce.Modules.Sale.Application.Commands.ValidateSaleStock;

public sealed class ValidateSaleStockHandler : IRequestHandler<ValidateSaleStockCommand, ApiResponse<SaleStockValidationDto>>
{
    private readonly SaleSubmissionService _saleSubmissionService;

    public ValidateSaleStockHandler(SaleSubmissionService saleSubmissionService)
    {
        _saleSubmissionService = saleSubmissionService ?? throw new ArgumentNullException(nameof(saleSubmissionService));
    }

    public async Task<ApiResponse<SaleStockValidationDto>> Handle(ValidateSaleStockCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _saleSubmissionService.GetSaleForSubmissionAsync(command.SaleId, cancellationToken);
        SaleSubmissionService.EnsureCanValidateStock(sale);

        SaleStockValidationDto result;
        try
        {
            result = await _saleSubmissionService.ReserveStockAsync(sale, cancellationToken);
        }
        catch (ConflictAppException exception)
        {
            result = new SaleStockValidationDto(
                sale.Id,
                false,
                false,
                "preview_only",
                null,
                sale.Items
                    .Where(static item => item.VariantId.HasValue)
                    .Select(item => new SaleStockValidationItemDto(
                        item.VariantId!.Value,
                        item.Quantity,
                        false,
                        exception.Message))
                    .ToArray());
        }

        return new ApiResponse<SaleStockValidationDto>
        {
            Status = StatusCodes.Status200OK,
            Data = result
        };
    }
}
