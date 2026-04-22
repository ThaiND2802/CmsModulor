using Commerce.Modules.Identity.Application.Commands.SyncUserRoles;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetUserRoles;
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
[Route("api/identity/users/{userId:guid}/roles")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityUserRolesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityUserRolesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize("Identity.UserRoles.Read")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleAssignmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserRoleAssignmentsResponse>>> GetUserRoles(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserRolesQuery
        {
            UserId = userId
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPut]
    [PermissionAuthorize("Identity.UserRoles.Assign")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleAssignmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserRoleAssignmentsResponse>>> SyncUserRoles(
        Guid userId,
        [FromBody] SyncUserRolesCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { UserId = userId }, cancellationToken);
        return StatusCode(response.Status, response);
    }
}
