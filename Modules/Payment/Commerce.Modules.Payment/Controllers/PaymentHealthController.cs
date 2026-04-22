using CommerceCore.Application.Responses;
using CommerceCore.FeatureManagement.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Payment.Controllers;

[ApiController]
[Route("api/payment")]
[RequireModule("Payment")]
public sealed class PaymentHealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new ApiResponse<PaymentHealthResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new PaymentHealthResponse("Payment", "Healthy")
        });
    }

    public sealed record PaymentHealthResponse(string Module, string Status);
}
