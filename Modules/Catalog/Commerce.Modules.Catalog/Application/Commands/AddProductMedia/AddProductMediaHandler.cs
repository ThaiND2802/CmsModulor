using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.AddProductMedia;

public sealed class AddProductMediaHandler : IRequestHandler<AddProductMediaCommand, Guid>
{
    private readonly CatalogDbContext _dbContext;

    public AddProductMediaHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Guid> Handle(AddProductMediaCommand command, CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products.AnyAsync(x => x.Id == command.ProductId, cancellationToken);
        if (!productExists)
        {
            throw new NotFoundAppException($"Product '{command.ProductId}' was not found.");
        }

        if (command.IsPrimary)
        {
            var existingPrimaryMedia = await _dbContext.ProductMedia.Where(x => x.ProductId == command.ProductId && x.IsPrimary).ToListAsync(cancellationToken);
            foreach (var media in existingPrimaryMedia)
            {
                media.IsPrimary = false;
            }
        }

        var nextDisplayOrder = await _dbContext.ProductMedia
            .Where(x => x.ProductId == command.ProductId)
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var mediaEntity = new ProductMedia
        {
            Id = Guid.NewGuid(),
            ProductId = command.ProductId,
            Url = command.Url.Trim(),
            AltText = string.IsNullOrWhiteSpace(command.AltText) ? null : command.AltText.Trim(),
            MediaType = command.MediaType,
            DisplayOrder = nextDisplayOrder + 1,
            IsPrimary = command.IsPrimary
        };

        await _dbContext.ProductMedia.AddAsync(mediaEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return mediaEntity.Id;
    }
}
