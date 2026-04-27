using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteCategory;

public sealed class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly CatalogDbContext _dbContext;

    public DeleteCategoryHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Category '{command.Id}' was not found.");

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
