using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetAuthorizationOverview;
using Commerce.Modules.Identity.Application.Queries.GetUsers;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityAuthorizationController : ApiControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    private readonly GetUsersHandler _getUsersHandler;
    private readonly GetAuthorizationOverviewHandler _getAuthorizationOverviewHandler;

    public IdentityAuthorizationController(
        GetUsersHandler getUsersHandler,
        GetAuthorizationOverviewHandler getAuthorizationOverviewHandler)
    {
        _getUsersHandler = getUsersHandler ?? throw new ArgumentNullException(nameof(getUsersHandler));
        _getAuthorizationOverviewHandler = getAuthorizationOverviewHandler ?? throw new ArgumentNullException(nameof(getAuthorizationOverviewHandler));
    }

    [HttpGet("users")]
    [PermissionAuthorize("Identity.Users.Read")]
    [ProducesResponseType(typeof(PagedApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<UserDto>>> GetUsers(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _getUsersHandler.HandleAsync(new GetUsersQuery(page, pageSize), cancellationToken);
        return PagedResponse(result.Users, result.Total, result.Page, result.PageSize);
    }

    [HttpGet("authorization/overview")]
    [PermissionAuthorize("Identity.Authorization.Read")]
    [ProducesResponseType(typeof(ApiResponse<AuthorizationOverviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthorizationOverviewDto>>> GetAuthorizationOverview(
        CancellationToken cancellationToken)
    {
        var response = await _getAuthorizationOverviewHandler.HandleAsync(new GetAuthorizationOverviewQuery(), cancellationToken);
        return SuccessResponse(response);
    }
}
