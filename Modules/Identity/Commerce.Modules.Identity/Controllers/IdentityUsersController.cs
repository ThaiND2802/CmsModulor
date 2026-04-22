using Commerce.Modules.Identity.Application.Commands.CreateUser;
using Commerce.Modules.Identity.Application.Commands.DeleteUser;
using Commerce.Modules.Identity.Application.Commands.UpdateUser;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Queries.GetUserById;
using Commerce.Modules.Identity.Application.Queries.GetUsers;
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
[Route("api/identity/users")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityUsersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityUsersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize("Identity.Users.Read")]
    [ProducesResponseType(typeof(PagedApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<UserDto>>> GetUsers(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Identity.Users.Read")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserByIdQuery
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpPost]
    [PermissionAuthorize("Identity.Users.Create")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Created($"/api/identity/users/{response.Data!.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize("Identity.Users.Update")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command with { Id = id }, cancellationToken);
        return StatusCode(response.Status, response);
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Identity.Users.Delete")]
    [ProducesResponseType(typeof(ApiResponse<DeleteUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteUserResponse>>> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteUserCommand
        {
            Id = id
        }, cancellationToken);
        return StatusCode(response.Status, response);
    }
}
