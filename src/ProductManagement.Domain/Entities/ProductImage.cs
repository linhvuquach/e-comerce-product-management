using ProductManagement.Domain.Common;

namespace ProductManagement.Domain.Entities;

public sealed class ProductImage : BaseEntity
{
    private ProductImage() { } // EF Core

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid? VariantId { get; private set; }
    public string BlobUrl { get; private set; } = default!;
    public string? CdnUrl { get; private set; }
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    public static ProductImage Create(
        Guid productId,
        string blobUrl,
        string? cdnUrl = null,
        string? altText = null,
        int sortOrder = 0,
        bool isPrimary = false,
        Guid? variantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobUrl);

        return new ProductImage
        {
            ProductId = productId,
            BlobUrl = blobUrl,
            CdnUrl = cdnUrl,
            AltText = altText,
            SortOrder = sortOrder,
            IsPrimary = isPrimary,
            VariantId = variantId,
        };
    }

    public void UpdateSort(int sortOrder)
    {
        SortOrder = sortOrder;
        SetUpdatedAt();
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
        SetUpdatedAt();
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
        SetUpdatedAt();
    }
}
