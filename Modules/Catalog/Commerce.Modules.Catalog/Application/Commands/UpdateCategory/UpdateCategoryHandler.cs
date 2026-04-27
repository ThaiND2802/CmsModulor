using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateCategory;

public sealed class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly CatalogDbContext _dbContext;

    public UpdateCategoryHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Category '{command.Id}' was not found.");

        var slug = command.Slug.Trim();
        var slugExists = await _dbContext.Categories.IgnoreQueryFilters()
            .AnyAsync(x => x.Id != command.Id && x.Slug == slug, cancellationToken);
        if (slugExists)
        {
            throw new ConflictAppException($"Category slug '{slug}' already exists.");
        }

        if (command.ParentId.HasValue)
        {
            if (command.ParentId.Value == command.Id)
            {
                throw new ConflictAppException("Category cannot be its own parent.");
            }

            var parentCategory = await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == command.ParentId.Value, cancellationToken)
                ?? throw new NotFoundAppException($"Parent category '{command.ParentId}' was not found.");

            var ancestorIds = new HashSet<Guid> { parentCategory.Id };
            var currentParentId = parentCategory.ParentId;

            while (currentParentId.HasValue)
            {
                if (currentParentId.Value == command.Id)
                {
                    throw new ConflictAppException("Category hierarchy cannot contain cycles.");
                }

                if (!ancestorIds.Add(currentParentId.Value))
                {
                    throw new ConflictAppException("Category hierarchy cannot contain cycles.");
                }

                currentParentId = await _dbContext.Categories
                    .AsNoTracking()
                    .Where(x => x.Id == currentParentId.Value)
                    .Select(x => x.ParentId)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        category.Name = command.Name.Trim();
        category.Slug = slug;
        category.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        category.ParentId = command.ParentId;
        category.DisplayOrder = command.DisplayOrder;
        category.IsActive = command.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
