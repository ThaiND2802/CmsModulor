using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateBrand;

public sealed class UpdateBrandHandler : IRequestHandler<UpdateBrandCommand>
{
    private readonly CatalogDbContext _dbContext;

    public UpdateBrandHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(UpdateBrandCommand command, CancellationToken cancellationToken)
    {
        var brand = await _dbContext.Brands.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Brand '{command.Id}' was not found.");

        var slug = command.Slug.Trim();
        var slugExists = await _dbContext.Brands.IgnoreQueryFilters().AnyAsync(x => x.Id != command.Id && x.Slug == slug, cancellationToken);
        if (slugExists)
        {
            throw new ConflictAppException($"Brand slug '{slug}' already exists.");
        }

        brand.Name = command.Name.Trim();
        brand.Slug = slug;
        brand.LogoUrl = string.IsNullOrWhiteSpace(command.LogoUrl) ? null : command.LogoUrl.Trim();
        brand.Website = string.IsNullOrWhiteSpace(command.Website) ? null : command.Website.Trim();
        brand.IsActive = command.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
