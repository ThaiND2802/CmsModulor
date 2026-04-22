using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Mappings;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Application.Responses;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Queries.GetPaymentMethodById;

public sealed class GetPaymentMethodByIdHandler : IRequestHandler<GetPaymentMethodByIdQuery, ApiResponse<PaymentMethodDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentMethodByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<PaymentMethodDto>> Handle(GetPaymentMethodByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await _unitOfWork.Repository<PaymentMethod>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (entity is null)
        {
            throw new NotFoundAppException($"Payment method '{query.Id}' was not found.", "1404");
        }

        return new ApiResponse<PaymentMethodDto>
        {
            Status = 200,
            Data = entity.ToPaymentMethodDto()
        };
    }
}
