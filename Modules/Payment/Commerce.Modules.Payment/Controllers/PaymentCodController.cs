using Commerce.Modules.Payment.Application.Commands.CreateCodCheckout;
using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Queries.GetCodHealth;
using Commerce.Modules.Payment.Application.Queries.GetMyCodPermissions;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Payment.Controllers;

[ApiController]
[Route("api/payment/cod")]
[RequireModule("Payment")]
[SwaggerModuleTag("Payment")]
public sealed class PaymentCodController : ApiControllerBase
{
    private readonly GetCodHealthHandler _getCodHealthHandler;
    private readonly GetMyCodPermissionsHandler _getMyCodPermissionsHandler;
    private readonly CreateCodCheckoutHandler _createCodCheckoutHandler;

    public PaymentCodController(
        GetCodHealthHandler getCodHealthHandler,
        GetMyCodPermissionsHandler getMyCodPermissionsHandler,
        CreateCodCheckoutHandler createCodCheckoutHandler)
    {
        _getCodHealthHandler = getCodHealthHandler ?? throw new ArgumentNullException(nameof(getCodHealthHandler));
        _getMyCodPermissionsHandler = getMyCodPermissionsHandler ?? throw new ArgumentNullException(nameof(getMyCodPermissionsHandler));
        _createCodCheckoutHandler = createCodCheckoutHandler ?? throw new ArgumentNullException(nameof(createCodCheckoutHandler));
    }

    [HttpGet("health")]
    [RequireFeature("Payment.COD")]
    [PermissionAuthorize("Payment.COD.Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentCodHealthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentCodHealthResponse>>> GetHealth(CancellationToken cancellationToken)
    {
        var response = await _getCodHealthHandler.HandleAsync(new GetCodHealthQuery(), cancellationToken);
        return SuccessResponse(response);
    }

    [HttpGet("permissions/me")]
    [RequireFeature("Payment.COD")]
    [PermissionAuthorize("Payment.COD.Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentCodPermissionsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentCodPermissionsResponse>>> GetMyPermissions(
        CancellationToken cancellationToken)
    {
        var response = await _getMyCodPermissionsHandler.HandleAsync(new GetMyCodPermissionsQuery(), cancellationToken);
        return SuccessResponse(response);
    }

    [HttpPost("checkout")]
    [RequireFeature("Payment.COD")]
    [PermissionAuthorize("Payment.COD.Checkout")]
    [ProducesResponseType(typeof(ApiResponse<PaymentCodCheckoutResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentCodCheckoutResponse>>> CreateCodCheckout(CancellationToken cancellationToken)
    {
        var response = await _createCodCheckoutHandler.HandleAsync(new CreateCodCheckoutCommand(), cancellationToken);
        return SuccessResponse(response);
    }
}
