namespace Commerce.Modules.Sale.Application.Services;

public interface ISaleCouponService
{
    Task<SaleCouponDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
