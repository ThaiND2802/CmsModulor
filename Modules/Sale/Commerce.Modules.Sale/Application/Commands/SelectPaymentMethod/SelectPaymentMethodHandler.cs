using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.SelectPaymentMethod;

public sealed class SelectPaymentMethodHandler : IRequestHandler<SelectPaymentMethodCommand>
{
    private readonly SaleDbContext _context;

    public SelectPaymentMethodHandler(SaleDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SelectPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken);

        if (sale is null)
            throw new NotFoundAppException($"Sale {request.SaleId} not found");

        try
        {
            sale.SelectPaymentMethod(request.PaymentMethod);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleAppException(ex.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
