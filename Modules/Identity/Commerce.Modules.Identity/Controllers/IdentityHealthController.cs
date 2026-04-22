using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
[RequireModule("Identity")]
[SwaggerModuleTag("Identity")]
public sealed class IdentityHealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new ApiResponse<IdentityHealthResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new IdentityHealthResponse("Identity", "Healthy")
        });
    }

    public sealed record IdentityHealthResponse(string Module, string Status);
}
