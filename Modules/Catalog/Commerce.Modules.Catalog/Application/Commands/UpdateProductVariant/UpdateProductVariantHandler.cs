using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantHandler : IRequestHandler<UpdateProductVariantCommand>
{
    private readonly CatalogDbContext _dbContext;

    public UpdateProductVariantHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(UpdateProductVariantCommand command, CancellationToken cancellationToken)
    {
        var variant = await _dbContext.ProductVariants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ProductId == command.ProductId && x.Id == command.VariantId, cancellationToken)
            ?? throw new NotFoundAppException($"Product variant '{command.VariantId}' was not found for product '{command.ProductId}'.");

        var sku = command.Sku.Trim();
        if (await _dbContext.ProductVariants.IgnoreQueryFilters().AnyAsync(x => x.Id != command.VariantId && x.Sku == sku, cancellationToken))
        {
            throw new ConflictAppException($"Product variant SKU '{sku}' already exists.");
        }

        if (command.IsDefault)
        {
            var existingDefaults = await _dbContext.ProductVariants
                .Where(x => x.ProductId == command.ProductId && x.Id != command.VariantId && x.IsDefault)
                .ToListAsync(cancellationToken);
            foreach (var existingDefault in existingDefaults)
            {
                existingDefault.IsDefault = false;
            }
        }

        variant.Sku = sku;
        variant.Name = command.Name.Trim();
        variant.Price = command.Price;
        variant.CompareAtPrice = command.CompareAtPrice;
        variant.StockQuantity = command.StockQuantity;
        variant.IsDefault = command.IsDefault;
        variant.IsActive = command.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
