using CommerceCore.Application.Responses;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Commerce.Host.WebApi.OpenApi;

public sealed class ErrorResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        AddErrorResponse(operation, context, StatusCodes.Status400BadRequest);
        AddErrorResponse(operation, context, StatusCodes.Status500InternalServerError);

        var hasPermissionRequirement = metadata.OfType<RequirePermissionAttribute>().Any()
            || metadata.OfType<AuthorizeAttribute>()
                .Select(x => x.Policy)
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(static x => x!)
                .Any(x => x.StartsWith($"{PermissionAuthorizeAttribute.PolicyPrefix}:", StringComparison.Ordinal));

        if (hasPermissionRequirement)
        {
            AddErrorResponse(operation, context, StatusCodes.Status401Unauthorized);
            AddErrorResponse(operation, context, StatusCodes.Status403Forbidden);
        }

        if (metadata.OfType<RequireModuleAttribute>().Any() || metadata.OfType<RequireFeatureAttribute>().Any())
        {
            AddErrorResponse(operation, context, StatusCodes.Status404NotFound);
        }
    }

    private static void AddErrorResponse(
        OpenApiOperation operation,
        OperationFilterContext context,
        int statusCode)
    {
        operation.Responses ??= new OpenApiResponses();

        var responseKey = statusCode.ToString();
        if (operation.Responses.ContainsKey(responseKey))
        {
            return;
        }

        operation.Responses[responseKey] = new OpenApiResponse
        {
            Description = GetDescription(statusCode),
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.OrdinalIgnoreCase)
            {
                ["application/json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository)
                }
            }
        };
    }

    private static string GetDescription(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };
    }
}
