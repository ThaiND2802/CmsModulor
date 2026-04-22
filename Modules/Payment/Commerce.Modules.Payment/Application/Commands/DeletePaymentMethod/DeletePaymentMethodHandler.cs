using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Application.Responses;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Commands.DeletePaymentMethod;

public sealed class DeletePaymentMethodHandler : IRequestHandler<DeletePaymentMethodCommand, ApiResponse<DeletePaymentMethodResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePaymentMethodHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<DeletePaymentMethodResponse>> Handle(DeletePaymentMethodCommand command, CancellationToken cancellationToken)
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

        return new ApiResponse<DeletePaymentMethodResponse>
        {
            Status = 200,
            Data = new DeletePaymentMethodResponse
            {
                Id = entity.Id,
                Deleted = entity.IsDeleted
            }
        };
    }
}
