using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Mappings;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Application.Responses;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Commands.CreatePaymentMethod;

public sealed class CreatePaymentMethodHandler : IRequestHandler<CreatePaymentMethodCommand, ApiResponse<PaymentMethodDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePaymentMethodHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<PaymentMethodDto>> Handle(CreatePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            throw new ValidationAppException("Payment method code is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationAppException("Payment method name is required.");
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        var normalizedName = command.Name.Trim();
        var repository = _unitOfWork.Repository<PaymentMethod>();
        var codeExists = await repository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Code == normalizedCode, cancellationToken);

        if (codeExists)
        {
            throw new ConflictAppException($"Payment method code '{normalizedCode}' already exists.");
        }

        var entity = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = normalizedName,
            IsActive = command.IsActive
        };

        await repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = entity.ToPaymentMethodDto();
        return new ApiResponse<PaymentMethodDto>
        {
            Status = 201,
            Data = response
        };
    }
}
