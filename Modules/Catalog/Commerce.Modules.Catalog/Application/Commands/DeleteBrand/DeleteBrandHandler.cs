using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteBrand;

public sealed class DeleteBrandHandler : IRequestHandler<DeleteBrandCommand>
{
    private readonly CatalogDbContext _dbContext;

    public DeleteBrandHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(DeleteBrandCommand command, CancellationToken cancellationToken)
    {
        var brand = await _dbContext.Brands.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Brand '{command.Id}' was not found.");

        _dbContext.Brands.Remove(brand);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
