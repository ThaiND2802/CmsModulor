using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Commands.MarkSalePaid;

public sealed class MarkSalePaidHandler : IRequestHandler<MarkSalePaidCommand>
{
    private readonly SaleDbContext _context;

    public MarkSalePaidHandler(SaleDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarkSalePaidCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken);

        if (sale is null)
            throw new NotFoundAppException($"Sale {request.SaleId} not found");

        try
        {
            sale.MarkPaid(request.Amount, request.PaymentReference);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleAppException(ex.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
