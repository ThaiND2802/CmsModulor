using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Application.Services;

public interface ISalePricingService
{
    void Apply(SaleEntity sale);
}
