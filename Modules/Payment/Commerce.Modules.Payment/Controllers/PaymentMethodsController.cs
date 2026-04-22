using Commerce.Modules.Payment.Application.Commands.CreatePaymentMethod;
using Commerce.Modules.Payment.Application.Commands.DeletePaymentMethod;
using Commerce.Modules.Payment.Application.Commands.UpdatePaymentMethod;
using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Queries.GetDeletedPaymentMethods;
using Commerce.Modules.Payment.Application.Queries.GetPaymentMethodById;
using Commerce.Modules.Payment.Application.Queries.GetPaymentMethods;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Payment.Controllers;

[ApiController]
[Route("api/payment/methods")]
[RequireModule("Payment")]
[SwaggerModuleTag("Payment")]
public sealed class PaymentMethodsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PaymentMethodsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedApiResponse<PaymentMethodDto>>> GetPaymentMethods(
        [FromQuery] GetPaymentMethodsQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> GetPaymentMethodById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetPaymentMethodByIdQuery
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> CreatePaymentMethod(
        [FromBody] CreatePaymentMethodCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Created($"/api/payment/methods/{response.Data!.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> UpdatePaymentMethod(
        Guid id,
        [FromBody] UpdatePaymentMethodCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeletePaymentMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeletePaymentMethodResponse>>> DeletePaymentMethod(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeletePaymentMethodCommand
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedApiResponse<DeletedPaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedApiResponse<DeletedPaymentMethodDto>>> GetDeletedPaymentMethods(
        [FromQuery] GetDeletedPaymentMethodsQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }
}
