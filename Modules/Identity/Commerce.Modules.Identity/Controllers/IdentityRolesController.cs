using Commerce.Modules.Identity.Application.Commands.CreateRole;
using Commerce.Modules.Identity.Application.Commands.DeleteRole;
using Commerce.Modules.Identity.Application.Commands.UpdateRole;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetRoleById;
using Commerce.Modules.Identity.Application.Queries.GetRoles;
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
[Route("api/identity/roles")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityRolesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityRolesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize("Identity.Roles.Read")]
    [ProducesResponseType(typeof(PagedApiResponse<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<RoleDto>>> GetRoles(
        [FromQuery] GetRolesQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Identity.Roles.Read")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetRoleByIdQuery
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPost]
    [PermissionAuthorize("Identity.Roles.Create")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole(
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Created($"/api/identity/roles/{response.Data!.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize("Identity.Roles.Update")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Identity.Roles.Delete")]
    [ProducesResponseType(typeof(ApiResponse<DeleteRoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteRoleResponse>>> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteRoleCommand
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }
}
