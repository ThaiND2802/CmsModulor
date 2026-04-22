using Commerce.Modules.Identity.Application.Commands.SyncUserPermissions;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetUserPermissions;
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
[Route("api/identity/users/{userId:guid}/permissions")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityUserPermissionsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityUserPermissionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize("Identity.UserPermissions.Read")]
    [ProducesResponseType(typeof(ApiResponse<UserPermissionAssignmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserPermissionAssignmentsResponse>>> GetUserPermissions(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserPermissionsQuery
        {
            UserId = userId
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPut]
    [PermissionAuthorize("Identity.UserPermissions.Assign")]
    [ProducesResponseType(typeof(ApiResponse<UserPermissionAssignmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserPermissionAssignmentsResponse>>> SyncUserPermissions(
        Guid userId,
        [FromBody] SyncUserPermissionsCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { UserId = userId }, cancellationToken);
        return StatusCode(response.Status, response);
    }
}
