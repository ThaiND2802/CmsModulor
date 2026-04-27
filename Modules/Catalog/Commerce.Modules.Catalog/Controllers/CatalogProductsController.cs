using Commerce.Modules.Catalog.Application.Commands.AddProductMedia;
using Commerce.Modules.Catalog.Application.Commands.ChangeProductStatus;
using Commerce.Modules.Catalog.Application.Commands.CreateProduct;
using Commerce.Modules.Catalog.Application.Commands.CreateProductVariant;
using Commerce.Modules.Catalog.Application.Commands.DeleteProduct;
using Commerce.Modules.Catalog.Application.Commands.DeleteProductVariant;
using Commerce.Modules.Catalog.Application.Commands.RemoveProductMedia;
using Commerce.Modules.Catalog.Application.Commands.UpdateProduct;
using Commerce.Modules.Catalog.Application.Commands.UpdateProductVariant;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Application.Queries.GetProductById;
using Commerce.Modules.Catalog.Application.Queries.GetProductBySlug;
using Commerce.Modules.Catalog.Application.Queries.GetProducts;
using Commerce.Modules.Catalog.Application.Queries.GetProductVariants;
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
[Route("api/catalog/products")]
[RequireModule("Catalog")]
[SwaggerModuleTag("Catalog")]
public sealed class CatalogProductsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CatalogProductsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize(CatalogPermissions.ProductsView)]
    [ProducesResponseType(typeof(PagedApiResponse<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<ProductListItemDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] Domain.ProductStatus? status = null,
        [FromQuery] bool? isFeatured = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            Desc = desc,
            CategoryId = categoryId,
            BrandId = brandId,
            Status = status,
            IsFeatured = isFeatured
        };

        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.ProductsView)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetProductByIdQuery(id), cancellationToken));
    }

    [HttpGet("slug/{slug}")]
    [PermissionAuthorize(CatalogPermissions.ProductsView)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetBySlug(string slug, CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetProductBySlugQuery(slug), cancellationToken));
    }

    [HttpPost]
    [PermissionAuthorize(CatalogPermissions.ProductsCreate)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedResponse($"/api/catalog/products/{id}", id);
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.ProductsEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.ProductsDelete)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpPut("{id:guid}/status")]
    [PermissionAuthorize(CatalogPermissions.ProductsEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> ChangeStatus(
        Guid id,
        [FromBody] ChangeProductStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpPost("{id:guid}/media")]
    [PermissionAuthorize(CatalogPermissions.ProductsEdit)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Guid>>> AddMedia(
        Guid id,
        [FromBody] AddProductMediaCommand command,
        CancellationToken cancellationToken = default)
    {
        var mediaId = await _mediator.Send(command with { ProductId = id }, cancellationToken);
        return CreatedResponse($"/api/catalog/products/{id}/media/{mediaId}", mediaId);
    }

    [HttpDelete("{id:guid}/media/{mediaId:guid}")]
    [PermissionAuthorize(CatalogPermissions.ProductsEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> RemoveMedia(
        Guid id,
        Guid mediaId,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new RemoveProductMediaCommand(id, mediaId), cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpGet("{id:guid}/variants")]
    [PermissionAuthorize(CatalogPermissions.ProductsView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProductVariantDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductVariantDto>>>> GetVariants(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetProductVariantsQuery(id), cancellationToken));
    }

    [HttpPost("{id:guid}/variants")]
    [PermissionAuthorize(CatalogPermissions.ProductsCreate)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateVariant(
        Guid id,
        [FromBody] CreateProductVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        var variantId = await _mediator.Send(command with { ProductId = id }, cancellationToken);
        return CreatedResponse($"/api/catalog/products/{id}/variants/{variantId}", variantId);
    }

    [HttpPut("{id:guid}/variants/{variantId:guid}")]
    [PermissionAuthorize(CatalogPermissions.ProductsEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> UpdateVariant(
        Guid id,
        Guid variantId,
        [FromBody] UpdateProductVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { ProductId = id, VariantId = variantId }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpDelete("{id:guid}/variants/{variantId:guid}")]
    [PermissionAuthorize(CatalogPermissions.ProductsDelete)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteVariant(
        Guid id,
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteProductVariantCommand(id, variantId), cancellationToken);
        return SuccessResponse<object?>(null);
    }
}
