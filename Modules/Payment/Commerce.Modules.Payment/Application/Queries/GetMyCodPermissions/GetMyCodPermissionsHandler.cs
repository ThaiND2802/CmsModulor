using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Abstractions;
using CommerceCore.FeatureManagement.Abstractions;

namespace Commerce.Modules.Payment.Application.Queries.GetMyCodPermissions;

public sealed class GetMyCodPermissionsHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionGate _permissionGate;

    public GetMyCodPermissionsHandler(ICurrentUser currentUser, IPermissionGate permissionGate)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _permissionGate = permissionGate ?? throw new ArgumentNullException(nameof(permissionGate));
    }

    public async Task<PaymentCodPermissionsResponse> HandleAsync(
        GetMyCodPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var canRead = _currentUser.IsAuthenticated
            && await _permissionGate.HasPermissionAsync(_currentUser.UserId!, "Payment.COD.Read", cancellationToken);
        var canCheckout = _currentUser.IsAuthenticated
            && await _permissionGate.HasPermissionAsync(_currentUser.UserId!, "Payment.COD.Checkout", cancellationToken);

        return new PaymentCodPermissionsResponse(
            _currentUser.UserId,
            _currentUser.UserName,
            _currentUser.IsAuthenticated,
            new PaymentCodPermissionFlags(canRead, canCheckout));
    }
}
