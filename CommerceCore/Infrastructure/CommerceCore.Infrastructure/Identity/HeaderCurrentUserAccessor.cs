using System.Security.Claims;
using CommerceCore.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace CommerceCore.Infrastructure.Identity;

public sealed class HeaderCurrentUserAccessor : ICurrentUser
{
    private const string UserIdHeaderName = "X-Commerce-UserId";
    private const string UserNameHeaderName = "X-Commerce-UserName";
    private const string PreferredUserNameClaimType = "preferred_username";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostEnvironment _hostEnvironment;

    public HeaderCurrentUserAccessor(IHttpContextAccessor httpContextAccessor, IHostEnvironment hostEnvironment)
    {
        _httpContextAccessor = httpContextAccessor;
        _hostEnvironment = hostEnvironment;
    }

    public string? UserId => GetUserId();

    public string? UserName => GetUserName();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true
        || !string.IsNullOrWhiteSpace(UserId);

    private string? GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated == true)
        {
            var claimValue =
                principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(PreferredUserNameClaimType)
                ?? principal.FindFirstValue(ClaimTypes.Name)
                ?? principal.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(claimValue))
            {
                return claimValue;
            }
        }

        return GetHeaderValue(UserIdHeaderName);
    }

    private string? GetUserName()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated == true)
        {
            var claimValue =
                principal.FindFirstValue(PreferredUserNameClaimType)
                ?? principal.FindFirstValue(ClaimTypes.Name)
                ?? principal.FindFirstValue("sub")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(claimValue))
            {
                return claimValue;
            }
        }

        return GetHeaderValue(UserNameHeaderName);
    }

    private string? GetHeaderValue(string headerName)
    {
        if (!_hostEnvironment.IsDevelopment())
        {
            return null;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || !httpContext.Request.Headers.TryGetValue(headerName, out var values))
        {
            return null;
        }

        var value = values.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
