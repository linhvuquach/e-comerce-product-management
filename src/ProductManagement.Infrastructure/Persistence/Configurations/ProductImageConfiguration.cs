using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.VariantId).HasColumnName("variant_id");

        builder.Property(i => i.BlobUrl).HasColumnName("blob_url").HasMaxLength(1000).IsRequired();
        builder.Property(i => i.CdnUrl).HasColumnName("cdn_url").HasMaxLength(1000);
        builder.Property(i => i.AltText).HasColumnName("alt_text").HasMaxLength(500);
        builder.Property(i => i.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(i => i.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        // Mirror the Product soft-delete filter so direct queries on images
        // also exclude images belonging to soft-deleted products.
        builder.HasQueryFilter(i => !i.Product.IsDeleted);
    }
}
