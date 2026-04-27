using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Commerce.Host.WebApi.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private const string InternalServerErrorCode = "1500";
    private const string InternalServerErrorMessage = "An unexpected error occurred.";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, errorCode, message) = exception switch
        {
            ValidationAppException appException => (StatusCodes.Status400BadRequest, appException.ErrorCode, appException.Message),
            UnauthorizedAppException appException => (StatusCodes.Status401Unauthorized, appException.ErrorCode, appException.Message),
            ForbiddenAppException appException => (StatusCodes.Status403Forbidden, appException.ErrorCode, appException.Message),
            NotFoundAppException appException => (StatusCodes.Status404NotFound, appException.ErrorCode, appException.Message),
            ConflictAppException appException => (StatusCodes.Status409Conflict, appException.ErrorCode, appException.Message),
            BusinessRuleAppException appException => (StatusCodes.Status422UnprocessableEntity, appException.ErrorCode, appException.Message),
            _ => (StatusCodes.Status500InternalServerError, InternalServerErrorCode, InternalServerErrorMessage)
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for request {RequestPath}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled application exception for request {RequestPath}", context.Request.Path);
        }

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("The response has already started, the global exception middleware will not write the error response.");
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Status = status,
            ErrorCode = errorCode,
            Message = message,
            Errors = exception is ValidationAppException validationException
                ? validationException.Errors
                : null
        };

        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }
}
