using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Commands.DeletePaymentMethod;

public sealed class DeletePaymentMethodHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePaymentMethodHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<DeletePaymentMethodResponse> HandleAsync(DeletePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var repository = _unitOfWork.Repository<PaymentMethod>();
        var entity = await repository.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (entity is null)
        {
            throw new NotFoundAppException($"Payment method '{command.Id}' was not found.");
        }

        if (entity.IsDeleted)
        {
            throw new NotFoundAppException($"Payment method '{command.Id}' was not found.");
        }

        repository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeletePaymentMethodResponse(entity.Id, entity.IsDeleted);
    }
}
