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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreatePaymentMethodRequestDto = Commerce.Modules.Payment.Application.DTOs.Requests.CreatePaymentMethodRequest;
using UpdatePaymentMethodRequestDto = Commerce.Modules.Payment.Application.DTOs.Requests.UpdatePaymentMethodRequest;

namespace Commerce.Modules.Payment.Controllers;

[ApiController]
[Route("api/payment/methods")]
[RequireModule("Payment")]
[SwaggerModuleTag("Payment")]
public sealed class PaymentMethodsController : ApiControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    private readonly GetPaymentMethodsHandler _getPaymentMethodsHandler;
    private readonly GetPaymentMethodByIdHandler _getPaymentMethodByIdHandler;
    private readonly CreatePaymentMethodHandler _createPaymentMethodHandler;
    private readonly UpdatePaymentMethodHandler _updatePaymentMethodHandler;
    private readonly DeletePaymentMethodHandler _deletePaymentMethodHandler;
    private readonly GetDeletedPaymentMethodsHandler _getDeletedPaymentMethodsHandler;

    public PaymentMethodsController(
        GetPaymentMethodsHandler getPaymentMethodsHandler,
        GetPaymentMethodByIdHandler getPaymentMethodByIdHandler,
        CreatePaymentMethodHandler createPaymentMethodHandler,
        UpdatePaymentMethodHandler updatePaymentMethodHandler,
        DeletePaymentMethodHandler deletePaymentMethodHandler,
        GetDeletedPaymentMethodsHandler getDeletedPaymentMethodsHandler)
    {
        _getPaymentMethodsHandler = getPaymentMethodsHandler ?? throw new ArgumentNullException(nameof(getPaymentMethodsHandler));
        _getPaymentMethodByIdHandler = getPaymentMethodByIdHandler ?? throw new ArgumentNullException(nameof(getPaymentMethodByIdHandler));
        _createPaymentMethodHandler = createPaymentMethodHandler ?? throw new ArgumentNullException(nameof(createPaymentMethodHandler));
        _updatePaymentMethodHandler = updatePaymentMethodHandler ?? throw new ArgumentNullException(nameof(updatePaymentMethodHandler));
        _deletePaymentMethodHandler = deletePaymentMethodHandler ?? throw new ArgumentNullException(nameof(deletePaymentMethodHandler));
        _getDeletedPaymentMethodsHandler = getDeletedPaymentMethodsHandler ?? throw new ArgumentNullException(nameof(getDeletedPaymentMethodsHandler));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedApiResponse<PaymentMethodDto>>> GetPaymentMethods(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _getPaymentMethodsHandler.HandleAsync(new GetPaymentMethodsQuery(page, pageSize), cancellationToken);
        return PagedResponse(result.Items, result.Total, result.Page, result.PageSize);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> GetPaymentMethodById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _getPaymentMethodByIdHandler.HandleAsync(new GetPaymentMethodByIdQuery(id), cancellationToken);
        return SuccessResponse(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> CreatePaymentMethod(
        [FromBody] CreatePaymentMethodRequestDto request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new CreatePaymentMethodRequestDto(request.Code, request.Name, request.IsActive);
        var response = await _createPaymentMethodHandler.HandleAsync(new CreatePaymentMethodCommand(applicationRequest), cancellationToken);
        return CreatedResponse($"/api/payment/methods/{response.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> UpdatePaymentMethod(
        Guid id,
        [FromBody] UpdatePaymentMethodRequestDto request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new UpdatePaymentMethodRequestDto(request.Name, request.IsActive);
        var response = await _updatePaymentMethodHandler.HandleAsync(new UpdatePaymentMethodCommand(id, applicationRequest), cancellationToken);
        return SuccessResponse(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeletePaymentMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeletePaymentMethodResponse>>> DeletePaymentMethod(Guid id, CancellationToken cancellationToken)
    {
        var response = await _deletePaymentMethodHandler.HandleAsync(new DeletePaymentMethodCommand(id), cancellationToken);
        return SuccessResponse(response);
    }

    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedApiResponse<DeletedPaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedApiResponse<DeletedPaymentMethodDto>>> GetDeletedPaymentMethods(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _getDeletedPaymentMethodsHandler.HandleAsync(new GetDeletedPaymentMethodsQuery(page, pageSize), cancellationToken);
        return PagedResponse(result.Items, result.Total, result.Page, result.PageSize);
    }
}
