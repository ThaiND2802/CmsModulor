using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateProduct;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly CatalogDbContext _dbContext;

    public UpdateProductHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Product '{command.Id}' was not found.");

        var categoryExists = await _dbContext.Categories.AnyAsync(x => x.Id == command.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundAppException($"Category '{command.CategoryId}' was not found.");
        }

        if (command.BrandId.HasValue)
        {
            var brandExists = await _dbContext.Brands.AnyAsync(x => x.Id == command.BrandId.Value, cancellationToken);
            if (!brandExists)
            {
                throw new NotFoundAppException($"Brand '{command.BrandId}' was not found.");
            }
        }

        var slug = command.Slug.Trim();
        var sku = command.Sku.Trim();
        if (await _dbContext.Products.IgnoreQueryFilters().AnyAsync(x => x.Id != command.Id && x.Slug == slug, cancellationToken))
        {
            throw new ConflictAppException($"Product slug '{slug}' already exists.");
        }

        if (await _dbContext.Products.IgnoreQueryFilters().AnyAsync(x => x.Id != command.Id && x.Sku == sku, cancellationToken))
        {
            throw new ConflictAppException($"Product SKU '{sku}' already exists.");
        }

        product.Name = command.Name.Trim();
        product.Slug = slug;
        product.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        product.ShortDescription = string.IsNullOrWhiteSpace(command.ShortDescription) ? null : command.ShortDescription.Trim();
        product.Sku = sku;
        product.CategoryId = command.CategoryId;
        product.BrandId = command.BrandId;
        product.IsFeatured = command.IsFeatured;
        product.Tags = string.IsNullOrWhiteSpace(command.Tags) ? null : command.Tags.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
