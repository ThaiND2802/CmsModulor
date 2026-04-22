using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CommerceCore.FeatureManagement.Authorization;

public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith($"{PermissionAuthorizeAttribute.PolicyPrefix}:", StringComparison.Ordinal))
        {
            var permission = policyName.Substring($"{PermissionAuthorizeAttribute.PolicyPrefix}:".Length);

            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}
