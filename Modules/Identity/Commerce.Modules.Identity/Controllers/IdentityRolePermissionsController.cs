using Commerce.Modules.Identity.Application.Commands.SyncRolePermissions;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetRolePermissions;
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
[Route("api/identity/roles/{roleId:guid}/permissions")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityRolePermissionsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityRolePermissionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize("Identity.RolePermissions.Read")]
    [ProducesResponseType(typeof(ApiResponse<RolePermissionAssignmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RolePermissionAssignmentsResponse>>> GetRolePermissions(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetRolePermissionsQuery
        {
            RoleId = roleId
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPut]
    [PermissionAuthorize("Identity.RolePermissions.Assign")]
    [ProducesResponseType(typeof(ApiResponse<RolePermissionAssignmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RolePermissionAssignmentsResponse>>> SyncRolePermissions(
        Guid roleId,
        [FromBody] SyncRolePermissionsCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { RoleId = roleId }, cancellationToken);
        return StatusCode(response.Status, response);
    }
}
