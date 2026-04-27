using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.CreateCategory;

public sealed class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly CatalogDbContext _dbContext;

    public CreateCategoryHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Guid> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var slug = command.Slug.Trim();
        var exists = await _dbContext.Categories.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug, cancellationToken);
        if (exists)
        {
            throw new ConflictAppException($"Category slug '{slug}' already exists.");
        }

        if (command.ParentId.HasValue)
        {
            var parentExists = await _dbContext.Categories.AnyAsync(x => x.Id == command.ParentId.Value, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundAppException($"Parent category '{command.ParentId}' was not found.");
            }
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Slug = slug,
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            ParentId = command.ParentId,
            DisplayOrder = command.DisplayOrder,
            IsActive = true
        };

        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
