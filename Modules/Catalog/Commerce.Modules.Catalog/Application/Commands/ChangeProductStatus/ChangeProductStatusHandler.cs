using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.ChangeProductStatus;

public sealed class ChangeProductStatusHandler : IRequestHandler<ChangeProductStatusCommand>
{
    private readonly CatalogDbContext _dbContext;

    public ChangeProductStatusHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(ChangeProductStatusCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Product '{command.Id}' was not found.");

        product.Status = command.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
