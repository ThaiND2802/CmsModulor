using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.RemoveProductMedia;

public sealed class RemoveProductMediaHandler : IRequestHandler<RemoveProductMediaCommand>
{
    private readonly CatalogDbContext _dbContext;

    public RemoveProductMediaHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(RemoveProductMediaCommand command, CancellationToken cancellationToken)
    {
        var media = await _dbContext.ProductMedia
            .FirstOrDefaultAsync(x => x.ProductId == command.ProductId && x.Id == command.MediaId, cancellationToken)
            ?? throw new NotFoundAppException($"Product media '{command.MediaId}' was not found for product '{command.ProductId}'.");

        _dbContext.ProductMedia.Remove(media);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
