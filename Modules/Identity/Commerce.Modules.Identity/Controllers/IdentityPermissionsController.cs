using Commerce.Modules.Identity.Application.Commands.CreatePermission;
using Commerce.Modules.Identity.Application.Commands.DeletePermission;
using Commerce.Modules.Identity.Application.Commands.UpdatePermission;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetPermissionById;
using Commerce.Modules.Identity.Application.Queries.GetPermissions;
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
[Route("api/identity/permissions")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityPermissionsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityPermissionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize("Identity.Permissions.Read")]
    [ProducesResponseType(typeof(PagedApiResponse<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<PermissionDto>>> GetPermissions(
        [FromQuery] GetPermissionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Identity.Permissions.Read")]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> GetPermissionById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetPermissionByIdQuery
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPost]
    [PermissionAuthorize("Identity.Permissions.Create")]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> CreatePermission(
        [FromBody] CreatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Created($"/api/identity/permissions/{response.Data!.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize("Identity.Permissions.Update")]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> UpdatePermission(
        Guid id,
        [FromBody] UpdatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Identity.Permissions.Delete")]
    [ProducesResponseType(typeof(ApiResponse<DeletePermissionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeletePermissionResponse>>> DeletePermission(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeletePermissionCommand
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }
}
