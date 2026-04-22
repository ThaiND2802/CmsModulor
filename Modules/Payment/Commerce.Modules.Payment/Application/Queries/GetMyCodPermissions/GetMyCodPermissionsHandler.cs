using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.FeatureManagement.Abstractions;
using MediatR;

namespace Commerce.Modules.Payment.Application.Queries.GetMyCodPermissions;

public sealed class GetMyCodPermissionsHandler : IRequestHandler<GetMyCodPermissionsQuery, ApiResponse<PaymentCodPermissionsResponse>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionGate _permissionGate;

    public GetMyCodPermissionsHandler(ICurrentUser currentUser, IPermissionGate permissionGate)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _permissionGate = permissionGate ?? throw new ArgumentNullException(nameof(permissionGate));
    }

    public async Task<ApiResponse<PaymentCodPermissionsResponse>> Handle(
        GetMyCodPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var canRead = _currentUser.IsAuthenticated
            && await _permissionGate.HasPermissionAsync(_currentUser.UserId!, "Payment.COD.Read", cancellationToken);
        var canCheckout = _currentUser.IsAuthenticated
            && await _permissionGate.HasPermissionAsync(_currentUser.UserId!, "Payment.COD.Checkout", cancellationToken);

        return new ApiResponse<PaymentCodPermissionsResponse>
        {
            Status = 200,
            Data = new PaymentCodPermissionsResponse
            {
                UserId = _currentUser.UserId,
                UserName = _currentUser.UserName,
                IsAuthenticated = _currentUser.IsAuthenticated,
                Permissions = new PaymentCodPermissionFlags
                {
                    PaymentCodRead = canRead,
                    PaymentCodCheckout = canCheckout
                }
            }
        };
    }
}
