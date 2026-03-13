using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Infrastructure.Persistence.Configurations;

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.ProductId).HasColumnName("product_id").IsRequired();

        builder.Property(v => v.Sku)
            .HasColumnName("sku")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(v => v.Sku).IsUnique().HasDatabaseName("idx_variants_sku");

        builder.Property(v => v.Size)
            .HasColumnName("size")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ProductSize>(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(v => v.Color).HasColumnName("color").HasMaxLength(100);
        builder.Property(v => v.ColorHex).HasColumnName("color_hex").HasMaxLength(9);
        builder.Property(v => v.StockQuantity).HasColumnName("stock_qty").HasDefaultValue(0);
        builder.Property(v => v.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.OwnsOne(v => v.PriceOverride, money =>
        {
            money.Property(m => m.Amount).HasColumnName("price_override").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("price_override_currency").HasMaxLength(3).HasDefaultValue("USD");
        });

        builder.Property(v => v.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        builder.Property(v => v.CreatedAt).HasColumnName("created_at");
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(v => v.ProductId)
            .HasFilter("is_active = true")
            .HasDatabaseName("idx_variants_product");

        builder.HasMany(v => v.Images)
            .WithOne()
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.SetNull);

        // Mirror the Product soft-delete filter so direct queries on variants
        // also exclude variants belonging to soft-deleted products.
        builder.HasQueryFilter(v => !v.Product.IsDeleted);
    }
}
