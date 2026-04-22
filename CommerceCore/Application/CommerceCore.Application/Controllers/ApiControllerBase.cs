using CommerceCore.Application.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Application.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> SuccessResponse<T>(T data)
    {
        return Ok(new ApiResponse<T>
        {
            Status = StatusCodes.Status200OK,
            Data = data
        });
    }

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(string location, T data)
    {
        return Created(location, new ApiResponse<T>
        {
            Status = StatusCodes.Status201Created,
            Data = data
        });
    }

    protected ActionResult<PagedApiResponse<T>> PagedResponse<T>(
        IReadOnlyCollection<T> data,
        int total,
        int page,
        int pageSize)
    {
        return Ok(new PagedApiResponse<T>
        {
            Status = StatusCodes.Status200OK,
            Data = data,
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        });
    }
}
