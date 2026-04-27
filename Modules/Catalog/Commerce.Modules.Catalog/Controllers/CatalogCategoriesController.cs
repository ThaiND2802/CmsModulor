using Commerce.Modules.Catalog.Application.Commands.CreateCategory;
using Commerce.Modules.Catalog.Application.Commands.DeleteCategory;
using Commerce.Modules.Catalog.Application.Commands.UpdateCategory;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Application.Queries.GetCategories;
using Commerce.Modules.Catalog.Application.Queries.GetCategoryById;
using Commerce.Modules.Catalog.Contracts;
using CommerceCore.Application.Controllers;
using CommerceCore.Application.Responses;
using CommerceCore.Application.Swagger;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Modules.Catalog.Controllers;

[ApiController]
[Route("api/catalog/categories")]
[RequireModule("Catalog")]
[SwaggerModuleTag("Catalog")]
public sealed class CatalogCategoriesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CatalogCategoriesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize(CatalogPermissions.CategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetAll(
        [FromQuery] bool treeView = true,
        CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetCategoriesQuery(treeView), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.CategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [PermissionAuthorize(CatalogPermissions.CategoriesCreate)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedResponse($"/api/catalog/categories/{id}", id);
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.CategoriesEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> Update(
        Guid id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.CategoriesDelete)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return SuccessResponse<object?>(null);
    }
}
