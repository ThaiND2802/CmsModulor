using Commerce.Modules.Order.Application.Commands.CancelOrder;
using Commerce.Modules.Order.Application.Commands.ChangeOrderStatus;
using Commerce.Modules.Order.Application.Commands.ConfirmOrder;
using Commerce.Modules.Order.Application.Commands.CreateOrder;
using Commerce.Modules.Order.Application.Commands.UpdateOrderNotes;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Application.Queries.GetOrderById;
using Commerce.Modules.Order.Application.Queries.GetOrderByNumber;
using Commerce.Modules.Order.Application.Queries.GetOrders;
using Commerce.Modules.Order.Application.Queries.GetOrdersByCustomer;
using Commerce.Modules.Order.Contracts;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Order.Controllers;

[ApiController]
[Route("api/order/orders")]
[RequireModule("Order")]
[SwaggerModuleTag("Order")]
public sealed class OrdersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize(OrderPermissions.OrdersView)]
    [ProducesResponseType(typeof(PagedApiResponse<OrderListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<OrderListItemDto>>> GetOrders(
        [FromQuery] GetOrdersQuery query,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(OrderPermissions.OrdersView)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetOrderByIdQuery { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("number/{orderNumber}")]
    [PermissionAuthorize(OrderPermissions.OrdersView)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetByNumber(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetOrderByNumberQuery { OrderNumber = orderNumber }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("customer/{customerId:guid}")]
    [PermissionAuthorize(OrderPermissions.OrdersView)]
    [ProducesResponseType(typeof(PagedApiResponse<OrderListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<OrderListItemDto>>> GetByCustomer(
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(
            new GetOrdersByCustomerQuery { CustomerId = customerId, Page = page, PageSize = pageSize },
            cancellationToken));

    [HttpPost]
    [PermissionAuthorize(OrderPermissions.OrdersCreate)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        if (response.Status != StatusCodes.Status201Created || response.Data is null)
        {
            return StatusCode(response.Status, response);
        }

        return Created($"/api/order/orders/{response.Data.Id}", response);
    }


    [HttpPost("{id:guid}/confirm")]
    [PermissionAuthorize(OrderPermissions.OrdersConfirm)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> ConfirmOrder(
        Guid id,
        [FromBody] ConfirmOrderCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPut("{id:guid}/status")]
    [PermissionAuthorize(OrderPermissions.OrdersUpdateStatus)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> ChangeStatus(
        Guid id,
        [FromBody] ChangeOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPost("{id:guid}/cancel")]
    [PermissionAuthorize(OrderPermissions.OrdersCancel)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(
        Guid id,
        [FromBody] CancelOrderCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPatch("{id:guid}/notes")]
    [PermissionAuthorize(OrderPermissions.OrdersUpdateNotes)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateNotes(
        Guid id,
        [FromBody] UpdateOrderNotesCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }
}
