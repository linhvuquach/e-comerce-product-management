using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasColumnName("slug")
            .HasMaxLength(350)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => Slug.FromRaw(v));

        builder.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("idx_products_slug");

        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.Brand).HasColumnName("brand").HasMaxLength(200).IsRequired();
        builder.Property(p => p.CategoryId).HasColumnName("category_id").IsRequired();

        // Money value object — owned type flattened to two columns
        builder.OwnsOne(p => p.BasePrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("base_price").HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("USD").IsRequired();
        });

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ProductStatus>(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        // tsvector — DB-maintained computed column (converted to GENERATED ALWAYS STORED
        // via AddSearchVectorTriggerAndSeedData migration). EF never writes to it.
        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasColumnName("search_vector")
            .HasColumnType("tsvector")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex("SearchVector")
            .HasMethod("GIN")
            .HasDatabaseName("idx_products_search_vector");

        // xmin system column as optimistic concurrency token (PostgreSQL-native)
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId);

        builder.HasIndex(p => new { p.CategoryId, p.Status })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_products_category_status");

        builder.HasIndex(p => p.DeletedAt)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_products_base_price");

        builder.HasQueryFilter(p=> !p.IsDeleted);
    }
}
