using System.ComponentModel.DataAnnotations;
using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Payment.Application.Queries.GetPaymentMethods;

public sealed record GetPaymentMethodsQuery : IRequest<PagedApiResponse<PaymentMethodDto>>
{
    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "page_size")]
    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}
