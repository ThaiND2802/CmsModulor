using CommerceCore.Application.Abstractions;
using CommerceCore.FeatureManagement.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace CommerceCore.FeatureManagement.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionGate _permissionGate;
    private readonly ICurrentUser _currentUser;

    public PermissionAuthorizationHandler(
        IPermissionGate permissionGate,
        ICurrentUser currentUser)
    {
        _permissionGate = permissionGate ?? throw new ArgumentNullException(nameof(permissionGate));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return;
        }

        var allowed = await _permissionGate.HasPermissionAsync(
            _currentUser.UserId,
            requirement.Permission);

        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}
