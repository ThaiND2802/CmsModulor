using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Mappings;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Commands.UpdatePaymentMethod;

public sealed class UpdatePaymentMethodHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentMethodHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PaymentMethodDto> HandleAsync(UpdatePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Request);

        if (string.IsNullOrWhiteSpace(command.Request.Name))
        {
            throw new ValidationAppException("Payment method name is required.");
        }

        var repository = _unitOfWork.Repository<PaymentMethod>();
        var entity = await repository.Query()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity is null)
        {
            throw new NotFoundAppException($"Payment method '{command.Id}' was not found.");
        }

        var normalizedName = command.Request.Name.Trim();
        var duplicateNameExists = await repository.Query()
            .AnyAsync(x => x.Id != command.Id && x.Name == normalizedName, cancellationToken);

        if (duplicateNameExists)
        {
            throw new ConflictAppException($"Payment method name '{normalizedName}' already exists.");
        }

        entity.Name = normalizedName;
        entity.IsActive = command.Request.IsActive;

        repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToPaymentMethodDto();
    }
}
