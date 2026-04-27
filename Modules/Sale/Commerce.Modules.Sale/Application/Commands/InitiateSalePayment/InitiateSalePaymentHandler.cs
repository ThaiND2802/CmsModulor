using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.InitiateSalePayment;

public sealed class InitiateSalePaymentHandler : IRequestHandler<InitiateSalePaymentCommand>
{
    private readonly SaleDbContext _context;

    public InitiateSalePaymentHandler(SaleDbContext context)
    {
        _context = context;
    }

    public async Task Handle(InitiateSalePaymentCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken);

        if (sale is null)
            throw new NotFoundAppException($"Sale {request.SaleId} not found");

        try
        {
            sale.InitiatePayment(request.PaymentReference);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleAppException(ex.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
