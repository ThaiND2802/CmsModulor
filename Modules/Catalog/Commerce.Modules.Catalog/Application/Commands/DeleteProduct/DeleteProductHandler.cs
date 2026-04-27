using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteProduct;

public sealed class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly CatalogDbContext _dbContext;

    public DeleteProductHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .IgnoreQueryFilters()
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Product '{command.Id}' was not found.");

        product.IsFeatured = false;
        foreach (var variant in product.Variants)
        {
            variant.IsActive = false;
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
