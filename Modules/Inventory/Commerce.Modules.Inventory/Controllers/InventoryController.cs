using Commerce.Modules.Inventory.Application.Commands.AdjustStock;
using Commerce.Modules.Inventory.Application.Commands.ReceiveStock;
using Commerce.Modules.Inventory.Application.Commands.ReleaseStock;
using Commerce.Modules.Inventory.Application.Commands.ReserveStock;
using Commerce.Modules.Inventory.Application.DTOs.Responses;
using Commerce.Modules.Inventory.Application.Queries.GetStockByVariant;
using Commerce.Modules.Inventory.Application.Queries.GetStockList;
using Commerce.Modules.Inventory.Application.Queries.GetStockMovements;
using Commerce.Modules.Inventory.Contracts;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Inventory.Controllers;

[ApiController]
[Route("api/inventory/stock")]
[RequireModule("Inventory")]
[SwaggerModuleTag("Inventory")]
public sealed class InventoryController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("receive")]
    [PermissionAuthorize(InventoryPermissions.StockReceive)]
    [ProducesResponseType(typeof(ApiResponse<InventoryStockDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<InventoryStockDto>>> Receive(
        [FromBody] ReceiveStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(command, cancellationToken);
        if (response.Data is null)
        {
            return StatusCode(response.Status, response);
        }

        return Created($"/api/inventory/stock/variants/{response.Data.VariantId}", response);
    }

    [HttpPost("adjust")]
    [PermissionAuthorize(InventoryPermissions.StockAdjust)]
    [ProducesResponseType(typeof(ApiResponse<InventoryStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventoryStockDto>>> Adjust(
        [FromBody] AdjustStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("variants/{variantId:guid}")]
    [PermissionAuthorize(InventoryPermissions.StockView)]
    [ProducesResponseType(typeof(ApiResponse<InventoryStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventoryStockDto>>> GetByVariant(Guid variantId, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetStockByVariantQuery(variantId), cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet]
    [PermissionAuthorize(InventoryPermissions.StockView)]
    [ProducesResponseType(typeof(PagedApiResponse<InventoryStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<InventoryStockDto>>> GetList(
        [FromQuery] GetStockListQuery query,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpPost("~/api/inventory/reservations")]
    [PermissionAuthorize(InventoryPermissions.ReservationsCreate)]
    [ProducesResponseType(typeof(ApiResponse<StockReservationDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<StockReservationDto>>> Reserve(
        [FromBody] ReserveStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPost("~/api/inventory/reservations/{orderId:guid}/release")]
    [PermissionAuthorize(InventoryPermissions.ReservationsRelease)]
    [ProducesResponseType(typeof(ApiResponse<StockReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StockReservationDto>>> Release(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new ReleaseStockCommand(orderId), cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("variants/{variantId:guid}/movements")]
    [PermissionAuthorize(InventoryPermissions.StockViewMovements)]
    [ProducesResponseType(typeof(PagedApiResponse<StockMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<StockMovementDto>>> GetMovements(
        Guid variantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetStockMovementsQuery { VariantId = variantId, Page = page, PageSize = pageSize }, cancellationToken));
}
