using Microsoft.AspNetCore.Authorization;

namespace CommerceCore.FeatureManagement.Authorization;

public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission";

    public PermissionAuthorizeAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        Policy = $"{PolicyPrefix}:{permission}";
    }
}
