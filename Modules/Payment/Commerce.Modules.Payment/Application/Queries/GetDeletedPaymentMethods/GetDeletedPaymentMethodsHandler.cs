using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Mappings;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Queries.GetDeletedPaymentMethods;

public sealed class GetDeletedPaymentMethodsHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDeletedPaymentMethodsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<(IReadOnlyCollection<DeletedPaymentMethodDto> Items, int Total, int Page, int PageSize)> HandleAsync(
        GetDeletedPaymentMethodsQuery query,
        CancellationToken cancellationToken)
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
        var methodsQuery = repository.Query()
            .IgnoreQueryFilters()
            .Where(static x => x.IsDeleted)
            .OrderBy(static x => x.Code);
        var total = await methodsQuery.CountAsync(cancellationToken);
        var methods = await methodsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (methods.Select(static x => x.ToDeletedPaymentMethodDto()).ToList(), total, query.Page, query.PageSize);
    }
}
