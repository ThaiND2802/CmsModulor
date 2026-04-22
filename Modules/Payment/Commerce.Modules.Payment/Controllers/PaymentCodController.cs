using Commerce.Modules.Payment.Application.Commands.CreateCodCheckout;
using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Queries.GetCodHealth;
using Commerce.Modules.Payment.Application.Queries.GetMyCodPermissions;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Payment.Controllers;

[ApiController]
[Route("api/payment/cod")]
[RequireModule("Payment")]
[SwaggerModuleTag("Payment")]
public sealed class PaymentCodController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PaymentCodController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("health")]
    [RequireFeature("Payment.COD")]
    [PermissionAuthorize("Payment.COD.Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentCodHealthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentCodHealthResponse>>> GetHealth(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCodHealthQuery(), cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("permissions/me")]
    [RequireFeature("Payment.COD")]
    [PermissionAuthorize("Payment.COD.Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentCodPermissionsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentCodPermissionsResponse>>> GetMyPermissions(
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetMyCodPermissionsQuery(), cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPost("checkout")]
    [RequireFeature("Payment.COD")]
    [PermissionAuthorize("Payment.COD.Checkout")]
    [ProducesResponseType(typeof(ApiResponse<PaymentCodCheckoutResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentCodCheckoutResponse>>> CreateCodCheckout(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new CreateCodCheckoutCommand(), cancellationToken);
        return StatusCode(response.Status, response);
    }
}
