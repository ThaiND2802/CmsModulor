using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.CreateProduct;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly CatalogDbContext _dbContext;

    public CreateProductHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
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
        if (await _dbContext.Products.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            throw new ConflictAppException($"Product slug '{slug}' already exists.");
        }

        if (await _dbContext.Products.IgnoreQueryFilters().AnyAsync(x => x.Sku == sku, cancellationToken))
        {
            throw new ConflictAppException($"Product SKU '{sku}' already exists.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Slug = slug,
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            ShortDescription = string.IsNullOrWhiteSpace(command.ShortDescription) ? null : command.ShortDescription.Trim(),
            Sku = sku,
            CategoryId = command.CategoryId,
            BrandId = command.BrandId,
            Status = ProductStatus.Draft,
            IsFeatured = command.IsFeatured,
            Tags = string.IsNullOrWhiteSpace(command.Tags) ? null : command.Tags.Trim()
        };

        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
