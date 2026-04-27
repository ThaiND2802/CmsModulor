using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteProductVariant;

public sealed class DeleteProductVariantHandler : IRequestHandler<DeleteProductVariantCommand>
{
    private readonly CatalogDbContext _dbContext;

    public DeleteProductVariantHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(DeleteProductVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == command.ProductId, cancellationToken)
            ?? throw new NotFoundAppException($"Product '{command.ProductId}' was not found.");

        var variant = product.Variants.FirstOrDefault(x => x.Id == command.VariantId)
            ?? throw new NotFoundAppException($"Product variant '{command.VariantId}' was not found for product '{command.ProductId}'.");

        if (product.Status == ProductStatus.Active && product.Variants.Count(x => !x.IsDeleted) <= 1)
        {
            throw new ConflictAppException("Cannot delete the last variant of an active product.");
        }

        _dbContext.ProductVariants.Remove(variant);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
