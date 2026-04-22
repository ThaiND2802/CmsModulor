using Commerce.Modules.Identity.Application.Commands.Login;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityAuthController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityAuthController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return StatusCode(response.Status, response);
    }

}
