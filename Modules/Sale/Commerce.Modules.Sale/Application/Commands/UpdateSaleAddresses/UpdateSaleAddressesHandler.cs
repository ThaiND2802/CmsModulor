using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleAddresses;

public sealed class UpdateSaleAddressesHandler : IRequestHandler<UpdateSaleAddressesCommand>
{
    private readonly SaleDbContext _dbContext;

    public UpdateSaleAddressesHandler(SaleDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(UpdateSaleAddressesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _dbContext.Sales.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Sale '{command.Id}' was not found.");

        if (!sale.IsMutable())
        {
            throw new ValidationAppException($"Sale '{command.Id}' is no longer editable.");
        }

        sale.ShippingAddress = MapAddress(command.ShippingAddress);
        sale.BillingAddress = MapAddress(command.BillingAddress);
        sale.Status = SaleStatus.Draft;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SaleAddress? MapAddress(UpdateSaleAddressRequest? request)
    {
        if (request is null)
        {
            return null;
        }

        return new SaleAddress
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            City = request.City.Trim(),
            State = request.State?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            Country = request.Country.Trim()
        };
    }
}
