using Commerce.Modules.Catalog.Application.Commands.DeleteCategory;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.DeleteCategory;

public sealed class DeleteCategoryHandlerTests
{
    [Fact]
    public async Task Handle_RemovesCategory_WhenFound()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        var handler = new DeleteCategoryHandler(dbContext);

        await handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        (await dbContext.Categories.IgnoreQueryFilters().SingleAsync(x => x.Id == category.Id)).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new DeleteCategoryHandler(dbContext);

        var act = () => handler.Handle(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
