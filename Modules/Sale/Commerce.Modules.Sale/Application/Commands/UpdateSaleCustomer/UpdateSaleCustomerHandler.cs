using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleCustomer;

public sealed class UpdateSaleCustomerHandler : IRequestHandler<UpdateSaleCustomerCommand>
{
    private readonly SaleDbContext _dbContext;

    public UpdateSaleCustomerHandler(SaleDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(UpdateSaleCustomerCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sale = await _dbContext.Sales.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Sale '{command.Id}' was not found.");

        if (!sale.IsMutable())
        {
            throw new ValidationAppException($"Sale '{command.Id}' is no longer editable.");
        }

        sale.CustomerId = command.CustomerId;
        sale.CustomerEmail = command.CustomerEmail.Trim();
        sale.CustomerPhone = command.CustomerPhone?.Trim();
        sale.Status = SaleStatus.Draft;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
