using Commerce.Host.WebApi.Configuration;
using Commerce.Host.WebApi.OpenApi;
using CommerceCore.Application.Swagger;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Commerce.Host.WebApi.Extensions;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureAwareSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OpenApiVisibilityOptions>(configuration.GetSection(OpenApiVisibilityOptions.SectionName));
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Commerce Host Web API",
                Version = "v1"
            });
            options.TagActionsBy(api =>
            {
                var actionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;

                var moduleTag =
                    actionDescriptor?.MethodInfo
                        .GetCustomAttributes(typeof(SwaggerModuleTagAttribute), false)
                        .Cast<SwaggerModuleTagAttribute>()
                        .FirstOrDefault()
                    ??
                    actionDescriptor?.ControllerTypeInfo
                        .GetCustomAttributes(typeof(SwaggerModuleTagAttribute), false)
                        .Cast<SwaggerModuleTagAttribute>()
                        .FirstOrDefault();

                if (moduleTag is not null)
                {
                    return [moduleTag.ModuleName];
                }

                return
                [
                    api.GroupName
                    ?? actionDescriptor?.ControllerName
                    ?? "Default"
                ];
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Chi nhap gia tri access token, Swagger se tu them tien to Bearer."
            });
            options.AddSecurityRequirement(static document => new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference("Bearer", document, null)
                ] = new List<string>()
            });
            options.OperationFilter<ErrorResponseOperationFilter>();
            options.OperationFilter<FeatureAwareSwaggerOperationFilter>();
            options.DocumentFilter<FeatureAwareSwaggerDocumentFilter>();
        });

        return services;
    }
}
