using Commerce.Modules.Catalog.Application.Commands.CreateBrand;
using Commerce.Modules.Catalog.Application.Commands.DeleteBrand;
using Commerce.Modules.Catalog.Application.Commands.UpdateBrand;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Application.Queries.GetBrandById;
using Commerce.Modules.Catalog.Application.Queries.GetBrands;
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
[Route("api/catalog/brands")]
[RequireModule("Catalog")]
[SwaggerModuleTag("Catalog")]
public sealed class CatalogBrandsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CatalogBrandsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [PermissionAuthorize(CatalogPermissions.BrandsView)]
    [ProducesResponseType(typeof(PagedApiResponse<BrandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<BrandDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBrandsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search
        };

        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.BrandsView)]
    [ProducesResponseType(typeof(ApiResponse<BrandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return SuccessResponse(await _mediator.Send(new GetBrandByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [PermissionAuthorize(CatalogPermissions.BrandsCreate)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateBrandCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedResponse($"/api/catalog/brands/{id}", id);
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.BrandsEdit)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> Update(
        Guid id,
        [FromBody] UpdateBrandCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return SuccessResponse<object?>(null);
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(CatalogPermissions.BrandsDelete)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteBrandCommand(id), cancellationToken);
        return SuccessResponse<object?>(null);
    }
}
