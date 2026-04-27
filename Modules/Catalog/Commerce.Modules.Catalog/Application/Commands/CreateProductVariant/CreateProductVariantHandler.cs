using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.CreateProductVariant;

public sealed class CreateProductVariantHandler : IRequestHandler<CreateProductVariantCommand, Guid>
{
    private readonly CatalogDbContext _dbContext;

    public CreateProductVariantHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Guid> Handle(CreateProductVariantCommand command, CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products.AnyAsync(x => x.Id == command.ProductId, cancellationToken);
        if (!productExists)
        {
            throw new NotFoundAppException($"Product '{command.ProductId}' was not found.");
        }

        var sku = command.Sku.Trim();
        if (await _dbContext.ProductVariants.IgnoreQueryFilters().AnyAsync(x => x.Sku == sku, cancellationToken))
        {
            throw new ConflictAppException($"Product variant SKU '{sku}' already exists.");
        }

        var attributeIds = command.Options.Select(x => x.AttributeId).Distinct().ToList();
        var existingAttributeIds = await _dbContext.ProductAttributes.Where(x => attributeIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
        if (existingAttributeIds.Count != attributeIds.Count)
        {
            throw new NotFoundAppException("One or more variant attributes were not found.");
        }

        if (command.IsDefault)
        {
            var existingDefaults = await _dbContext.ProductVariants.Where(x => x.ProductId == command.ProductId && x.IsDefault).ToListAsync(cancellationToken);
            foreach (var existingDefault in existingDefaults)
            {
                existingDefault.IsDefault = false;
            }
        }

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = command.ProductId,
            Sku = sku,
            Name = command.Name.Trim(),
            Price = command.Price,
            CompareAtPrice = command.CompareAtPrice,
            StockQuantity = command.StockQuantity,
            IsDefault = command.IsDefault,
            IsActive = true,
            Options = command.Options.Select(option => new ProductVariantOption
            {
                Id = Guid.NewGuid(),
                AttributeId = option.AttributeId,
                Value = option.Value.Trim()
            }).ToList()
        };

        await _dbContext.ProductVariants.AddAsync(variant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return variant.Id;
    }
}
