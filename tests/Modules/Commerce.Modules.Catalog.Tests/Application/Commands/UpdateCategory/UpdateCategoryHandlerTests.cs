using Commerce.Modules.Catalog.Application.Commands.UpdateCategory;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateCategory;

public sealed class UpdateCategoryHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesCategory_WhenValid()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var parent = new Category { Id = Guid.NewGuid(), Name = "Parent", Slug = "parent" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        dbContext.Categories.AddRange(parent, category);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateCategoryHandler(dbContext);

        await handler.Handle(new UpdateCategoryCommand(category.Id, " Boots ", " boots ", " Desc ", parent.Id, 2, false), CancellationToken.None);

        var updated = await dbContext.Categories.SingleAsync(x => x.Id == category.Id);
        updated.Name.Should().Be("Boots");
        updated.Slug.Should().Be("boots");
        updated.Description.Should().Be("Desc");
        updated.ParentId.Should().Be(parent.Id);
        updated.DisplayOrder.Should().Be(2);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenHierarchyWouldCycle()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var root = new Category { Id = Guid.NewGuid(), Name = "Root", Slug = "root" };
        var parent = new Category { Id = Guid.NewGuid(), Name = "Parent", Slug = "parent", ParentId = root.Id };
        var child = new Category { Id = Guid.NewGuid(), Name = "Child", Slug = "child", ParentId = parent.Id };
        dbContext.Categories.AddRange(root, parent, child);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateCategoryHandler(dbContext);

        var act = () => handler.Handle(new UpdateCategoryCommand(root.Id, "Root", "root", null, child.Id, 0, true), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
