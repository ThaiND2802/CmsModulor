using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.CreateBrand;

public sealed class CreateBrandHandler : IRequestHandler<CreateBrandCommand, Guid>
{
    private readonly CatalogDbContext _dbContext;

    public CreateBrandHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Guid> Handle(CreateBrandCommand command, CancellationToken cancellationToken)
    {
        var slug = command.Slug.Trim();
        var exists = await _dbContext.Brands.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug, cancellationToken);
        if (exists)
        {
            throw new ConflictAppException($"Brand slug '{slug}' already exists.");
        }

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Slug = slug,
            LogoUrl = string.IsNullOrWhiteSpace(command.LogoUrl) ? null : command.LogoUrl.Trim(),
            Website = string.IsNullOrWhiteSpace(command.Website) ? null : command.Website.Trim(),
            IsActive = command.IsActive
        };

        await _dbContext.Brands.AddAsync(brand, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return brand.Id;
    }
}
