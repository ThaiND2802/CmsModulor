using CommerceCore.SharedKernel.Exceptions;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.InitiateSalePayment;

public sealed class InitiateSalePaymentHandler : IRequestHandler<InitiateSalePaymentCommand>
{
    public Task Handle(InitiateSalePaymentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromException(new NotFoundAppException("Initiate payment is disabled in the MVP runtime."));
    }
}
