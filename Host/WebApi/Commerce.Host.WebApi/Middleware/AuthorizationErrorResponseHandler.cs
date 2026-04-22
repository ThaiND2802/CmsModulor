using CommerceCore.Application.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Commerce.Host.WebApi.Middleware;

public sealed class AuthorizationErrorResponseHandler : IAuthorizationMiddlewareResultHandler
{
    private static readonly AuthorizationMiddlewareResultHandler DefaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(authorizeResult);

        if (authorizeResult.Challenged)
        {
            await context.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = "1401",
                    Message = "Authentication is required to access this endpoint."
                }, context.RequestAborted);
            }

            return;
        }

        if (authorizeResult.Forbidden)
        {
            await context.ForbidAsync(JwtBearerDefaults.AuthenticationScheme);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = "1403",
                    Message = "You do not have permission to access this endpoint."
                }, context.RequestAborted);
            }

            return;
        }

        await DefaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
