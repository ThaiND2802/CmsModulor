using Commerce.Modules.Identity.Application.Commands.Login;
using Commerce.Modules.Identity.Application.DTOs.Requests;
using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityAuthController : ApiControllerBase
{
    private readonly LoginHandler _loginHandler;

    public IdentityAuthController(LoginHandler loginHandler)
    {
        _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _loginHandler.HandleAsync(new LoginCommand(request), cancellationToken);
        return SuccessResponse(response);
    }

}
