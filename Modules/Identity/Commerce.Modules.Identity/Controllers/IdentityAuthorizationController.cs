using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetAuthorizationOverview;
using Commerce.Modules.Identity.Application.Queries.GetMe;
using Commerce.Modules.Identity.Application.Queries.GetMyPermissions;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityAuthorizationController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityAuthorizationController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("me")]
    [PermissionAuthorize("Identity.Me.Read")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetMeQuery(), cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("my-permissions")]
    [PermissionAuthorize("Identity.MyPermissions.Read")]
    [ProducesResponseType(typeof(ApiResponse<MyPermissionsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MyPermissionsResponse>>> GetMyPermissions(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetMyPermissionsQuery(), cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpGet("authorization/overview")]
    [PermissionAuthorize("Identity.Authorization.Read")]
    [ProducesResponseType(typeof(ApiResponse<AuthorizationOverviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthorizationOverviewDto>>> GetAuthorizationOverview(
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAuthorizationOverviewQuery(), cancellationToken);
        return Ok(response);
    }
}
