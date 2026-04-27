using Commerce.Host.WebApi.Extensions;
using Commerce.Host.WebApi.Middleware;
using CommerceCore.Application.Responses;
using CommerceCore.FeatureManagement.Authorization;
using CommerceCore.FeatureManagement.DependencyInjection;
using CommerceCore.Infrastructure.DependencyInjection;
using CommerceCore.Observability.DependencyInjection;
using Commerce.Modules.Identity;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using Commerce.Modules.Catalog;
using Commerce.Modules.Pricing;
using Commerce.Modules.Sale;
using Commerce.Modules.Cart;
using Commerce.Modules.Order;
using Commerce.Modules.Payment;
using Commerce.Modules.Shipping;
using Commerce.Modules.Inventory;
using Commerce.Modules.Reporting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Default is missing.");
}

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");
var applyDatabaseChanges = builder.Configuration.GetValue<bool>("Startup:ApplyDatabaseChanges");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)
    || string.IsNullOrWhiteSpace(jwtOptions.Audience)
    || string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException("Jwt configuration is incomplete.");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Commerce.Modules.Identity.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Commerce.Modules.Payment.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Commerce.Modules.Catalog.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Commerce.Modules.Order.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Commerce.Modules.Inventory.DependencyInjection).Assembly)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var firstError = context.ModelState.Values
            .SelectMany(static value => value.Errors)
            .Select(static error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "The request payload is invalid."
                : error.ErrorMessage)
            .FirstOrDefault() ?? "The request payload is invalid.";

        var errors = context.ModelState
            .Where(static entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                static entry => entry.Key,
                static entry => (IReadOnlyList<string>)entry.Value!.Errors
                    .Select(static error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The request payload is invalid."
                        : error.ErrorMessage)
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        return new BadRequestObjectResult(new ErrorResponse
        {
            Status = StatusCodes.Status400BadRequest,
            ErrorCode = "1001",
            Message = firstError,
            Errors = errors
        });
    };
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "preferred_username",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationErrorResponseHandler>();
builder.Services.AddFeatureAwareSwagger(builder.Configuration);
builder.Services.AddCommerceInfrastructure(builder.Configuration);
builder.Services.AddCommerceFeatureManagement(builder.Configuration);
builder.Services.AddCommerceObservability();
builder.Services.AddModuleIfEnabled(builder.Configuration, "Identity", (services, configuration) => services.AddIdentityModule(configuration, connectionString));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Catalog", (services, configuration) => services.AddCatalogModule(configuration, connectionString));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Pricing", (services, configuration) => services.AddPricingModule(configuration));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Sale", (services, configuration) => services.AddSaleModule(configuration, connectionString));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Cart", (services, configuration) => services.AddCartModule(configuration));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Order", (services, configuration) => services.AddOrderModule(configuration, connectionString));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Payment", (services, configuration) => services.AddPaymentModule(configuration, connectionString));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Shipping", (services, configuration) => services.AddShippingModule(configuration));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Inventory", (services, configuration) => services.AddInventoryModule(configuration, connectionString));
builder.Services.AddModuleIfEnabled(builder.Configuration, "Reporting", (services, configuration) => services.AddReportingModule(configuration));

var app = builder.Build();

if (applyDatabaseChanges)
{
    await app.Services.MigrateIdentityModuleAsync();
    await app.Services.MigrateCatalogModuleAsync();
    await app.Services.MigrateSaleModuleAsync();
    await app.Services.MigrateOrderModuleAsync();
    await app.Services.MigrateInventoryModuleAsync();
    await app.Services.SeedSalePermissionsAsync();
    await app.Services.SeedOrderPermissionsAsync();
    await app.Services.SeedInventoryPermissionsAsync();
    await app.Services.InitializePaymentPersistenceAsync();
}

app.UseCorrelationId();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseFeatureGate();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Commerce.Host.WebApi",
    status = "Running",
    utc_now = DateTime.UtcNow
}));

app.MapControllers();
app.MapModuleLoadingEndpoints();
app.MapFeatureFlagEndpoints();
app.MapModuleIfEnabled("Catalog", endpoints => endpoints.MapCatalogEndpoints());
app.MapModuleIfEnabled("Pricing", endpoints => endpoints.MapPricingEndpoints());
app.MapModuleIfEnabled("Sale", endpoints => endpoints.MapSaleEndpoints());
app.MapModuleIfEnabled("Cart", endpoints => endpoints.MapCartEndpoints());
app.MapModuleIfEnabled("Order", endpoints => endpoints.MapOrderEndpoints());
app.MapModuleIfEnabled("Shipping", endpoints => endpoints.MapShippingEndpoints());
app.MapModuleIfEnabled("Inventory", endpoints => endpoints.MapInventoryEndpoints());
app.MapModuleIfEnabled("Reporting", endpoints => endpoints.MapReportingEndpoints());

app.Run();
