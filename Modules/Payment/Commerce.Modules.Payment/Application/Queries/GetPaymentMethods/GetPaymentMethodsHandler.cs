using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Mappings;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Queries.GetPaymentMethods;

public sealed class GetPaymentMethodsHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentMethodsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<GetPaymentMethodsResult> HandleAsync(GetPaymentMethodsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page <= 0)
        {
            throw new ValidationAppException("Page must be greater than 0.");
        }

        if (query.PageSize <= 0)
        {
            throw new ValidationAppException("Page size must be greater than 0.");
        }

        var repository = _unitOfWork.Repository<PaymentMethod>();
        var methodsQuery = repository.Query().OrderBy(static x => x.Code);
        var total = await methodsQuery.CountAsync(cancellationToken);
        var methods = await methodsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new GetPaymentMethodsResult(
            methods.Select(static x => x.ToPaymentMethodDto()).ToList(),
            total,
            query.Page,
            query.PageSize);
    }
}
