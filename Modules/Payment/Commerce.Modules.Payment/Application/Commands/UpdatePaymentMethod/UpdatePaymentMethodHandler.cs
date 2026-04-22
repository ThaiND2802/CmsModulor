using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Mappings;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Application.Responses;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Commands.UpdatePaymentMethod;

public sealed class UpdatePaymentMethodHandler : IRequestHandler<UpdatePaymentMethodCommand, ApiResponse<PaymentMethodDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentMethodHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<PaymentMethodDto>> Handle(UpdatePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
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

        var normalizedName = command.Name.Trim();
        var duplicateNameExists = await repository.Query()
            .AnyAsync(x => x.Id != command.Id && x.Name == normalizedName, cancellationToken);

        if (duplicateNameExists)
        {
            throw new ConflictAppException($"Payment method name '{normalizedName}' already exists.");
        }

        entity.Name = normalizedName;
        entity.IsActive = command.IsActive;

        repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<PaymentMethodDto>
        {
            Status = 200,
            Data = entity.ToPaymentMethodDto()
        };
    }
}
