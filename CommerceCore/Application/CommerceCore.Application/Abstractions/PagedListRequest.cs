using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Application.Abstractions;

public class PagedListRequest
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 200;

    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = DefaultPage;

    [FromQuery(Name = "page_size")]
    [Range(1, MaxPageSize)]
    public int PageSize { get; init; } = DefaultPageSize;

    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    [FromQuery(Name = "sort_by")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "desc")]
    public bool Desc { get; init; }

    public int NormalizedPage => Page < 1 ? DefaultPage : Page;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };
}
