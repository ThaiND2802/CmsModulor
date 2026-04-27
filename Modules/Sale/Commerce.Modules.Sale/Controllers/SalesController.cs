using Commerce.Modules.Sale.Application.Commands.AddSaleItem;
using Commerce.Modules.Sale.Application.Commands.ApplyCoupon;
using Commerce.Modules.Sale.Application.Commands.CancelSale;
using Commerce.Modules.Sale.Application.Commands.CreateSale;
using Commerce.Modules.Sale.Application.Commands.InitiateSalePayment;
using Commerce.Modules.Sale.Application.Commands.MarkSalePaid;
using Commerce.Modules.Sale.Application.Commands.OverrideSalePrice;
using Commerce.Modules.Sale.Application.Commands.RemoveSaleItem;
using Commerce.Modules.Sale.Application.Commands.RemoveCoupon;
using Commerce.Modules.Sale.Application.Commands.RepriceSale;
using Commerce.Modules.Sale.Application.Commands.SelectPaymentMethod;
using Commerce.Modules.Sale.Application.Commands.SubmitSale;
using Commerce.Modules.Sale.Application.Commands.UpdateSaleItem;
using Commerce.Modules.Sale.Application.Commands.UpdateSaleAddresses;
using Commerce.Modules.Sale.Application.Commands.UpdateSaleCustomer;
using Commerce.Modules.Sale.Application.Commands.ValidateSaleStock;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Application.Queries.GetSaleById;
using Commerce.Modules.Sale.Application.Queries.GetSales;
using Commerce.Modules.Sale.Application.Queries.GetSalesDashboard;
using Commerce.Modules.Sale.Application.Queries.GetSaleTimeline;
using Commerce.Modules.Sale.Contracts;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Sale.Controllers;

[ApiController]
[Route("api/sale/sales")]
[RequireModule("Sale")]
[SwaggerModuleTag("Sale")]
public sealed class SalesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SalesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    [PermissionAuthorize(SalePermissions.SalesCreate)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Create(
        [FromBody] CreateSaleCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Created($"/api/sale/sales/{response.Data!.Id}", response);
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(SalePermissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetSaleByIdQuery(id), cancellationToken));
    }

    [HttpGet]
    [PermissionAuthorize(SalePermissions.SalesView)]
    [ProducesResponseType(typeof(PagedApiResponse<SaleListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<SaleListItemDto>>> GetAll(
        [FromQuery] GetSalesQuery query,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpPatch("{id:guid}/customer")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> UpdateCustomer(
        Guid id,
        [FromBody] UpdateSaleCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpPatch("{id:guid}/addresses")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> UpdateAddresses(
        Guid id,
        [FromBody] UpdateSaleAddressesCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpPost("{id:guid}/items")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> AddItem(
        Guid id,
        [FromBody] AddSaleItemCommand command,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(command with { SaleId = id }, cancellationToken));
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> UpdateItem(
        Guid id,
        Guid itemId,
        [FromBody] UpdateSaleItemCommand command,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(command with { SaleId = id, ItemId = itemId }, cancellationToken));
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> RemoveItem(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new RemoveSaleItemCommand
        {
            SaleId = id,
            ItemId = itemId
        }, cancellationToken));
    }

    [HttpPost("{id:guid}/reprice")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Reprice(
        Guid id,
        [FromBody] RepriceSaleCommand command,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(command with { SaleId = id }, cancellationToken));
    }

    [HttpPost("{id:guid}/coupon")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> ApplyCoupon(
        Guid id,
        [FromBody] ApplyCouponCommand command,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(command with { SaleId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}/coupon")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> RemoveCoupon(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new RemoveCouponCommand { SaleId = id }, cancellationToken));
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/override-price")]
    [PermissionAuthorize(SalePermissions.SalesOverridePrice)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> OverridePrice(
        Guid id,
        Guid itemId,
        [FromBody] OverrideSalePriceCommand command,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(command with { SaleId = id, ItemId = itemId }, cancellationToken));
    }

    [HttpPost("{id:guid}/validate-stock")]
    [PermissionAuthorize(SalePermissions.SalesSubmit)]
    [ProducesResponseType(typeof(ApiResponse<SaleStockValidationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleStockValidationDto>>> ValidateStock(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new ValidateSaleStockCommand { SaleId = id }, cancellationToken));
    }

    [HttpPost("{id:guid}/submit")]
    [PermissionAuthorize(SalePermissions.SalesSubmit)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Submit(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new SubmitSaleCommand { SaleId = id }, cancellationToken));
    }

    [HttpPost("{id:guid}/cancel")]
    [PermissionAuthorize(SalePermissions.SalesCancel)]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Cancel(
        Guid id,
        [FromBody] CancelSaleCommand command,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(command with { SaleId = id }, cancellationToken));
    }

    [HttpPost("{id:guid}/payment-method")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> SelectPaymentMethod(
        Guid id,
        [FromBody] SelectPaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { SaleId = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpPost("{id:guid}/initiate-payment")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> InitiatePayment(
        Guid id,
        [FromBody] InitiateSalePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { SaleId = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpPost("{id:guid}/mark-paid")]
    [PermissionAuthorize(SalePermissions.SalesEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> MarkPaid(
        Guid id,
        [FromBody] MarkSalePaidCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { SaleId = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpGet("{id:guid}/timeline")]
    [PermissionAuthorize(SalePermissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<SaleTimelineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleTimelineDto>>> GetTimeline(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetSaleTimelineQuery(id), cancellationToken));
    }

    [HttpGet("dashboard")]
    [PermissionAuthorize(SalePermissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<SalesDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SalesDashboardDto>>> GetDashboard(
        CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetSalesDashboardQuery(), cancellationToken));
    }
}
