using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Application.Mappings;
using Commerce.Modules.Payment.Domain;
using CommerceCore.Application.Responses;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Application.Queries.GetPaymentMethods;

public sealed class GetPaymentMethodsHandler : IRequestHandler<GetPaymentMethodsQuery, PagedApiResponse<PaymentMethodDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentMethodsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PagedApiResponse<PaymentMethodDto>> Handle(GetPaymentMethodsQuery query, CancellationToken cancellationToken)
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

        return new PagedApiResponse<PaymentMethodDto>
        {
            Status = 200,
            Data = methods.Select(static x => x.ToPaymentMethodDto()).ToList(),
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize
            }
        };
    }
}
