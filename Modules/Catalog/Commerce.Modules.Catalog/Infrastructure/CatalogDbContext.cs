using Commerce.Modules.Catalog.Domain;
using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Infrastructure;

public sealed class CatalogDbContext : BaseDbContext
{
    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
        : base(options, dateTimeProvider, currentUser)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductMedia> ProductMedia => Set<ProductMedia>();

    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();

    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();

    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<ProductVariantOption> ProductVariantOptions => Set<ProductVariantOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ProductMedia>()
            .HasQueryFilter(static media => !media.Product.IsDeleted);
        modelBuilder.Entity<ProductAttributeValue>()
            .HasQueryFilter(static attributeValue => !attributeValue.Product.IsDeleted);
        modelBuilder.Entity<ProductVariantOption>()
            .HasQueryFilter(static option => !option.Variant.IsDeleted);
    }
}
