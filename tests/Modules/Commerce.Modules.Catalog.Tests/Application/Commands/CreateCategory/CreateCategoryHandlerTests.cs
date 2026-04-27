using Commerce.Modules.Catalog.Application.Commands.CreateCategory;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateCategory;

public sealed class CreateCategoryHandlerTests
{
    [Fact]
    public async Task Handle_CreatesCategory_WhenSlugIsUnique()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new CreateCategoryHandler(dbContext);
        var command = new CreateCategoryCommand("Shoes", "shoes", "desc", null, 0);

        var categoryId = await handler.Handle(command, CancellationToken.None);

        var category = await dbContext.Categories.SingleAsync(x => x.Id == categoryId);
        category.Name.Should().Be("Shoes");
        category.Slug.Should().Be("shoes");
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenSlugAlreadyExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        dbContext.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Existing", Slug = "shoes" });
        await dbContext.SaveChangesAsync();

        var handler = new CreateCategoryHandler(dbContext);
        var command = new CreateCategoryCommand("Shoes", "shoes", null, null, 0);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
