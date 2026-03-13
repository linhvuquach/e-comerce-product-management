using System.Text.Json;
using ProductManagement.Domain.Common;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Domain.Entities;

public sealed class ProductVariant : BaseEntity
{
    private readonly List<ProductImage> _images = [];

    private ProductVariant() { } // EF Core

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public string Sku { get; private set; } = default!;
    public ProductSize Size { get; private set; }
    public string? Color { get; private set; }
    public string? ColorHex { get; private set; }

    /// <summary>Null means use the parent product's base price.</summary>
    public Money? PriceOverride { get; private set; }

    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;
    public JsonDocument? Attributes { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    public static ProductVariant Create(
        Guid productId,
        string sku,
        ProductSize size,
        string? color = null,
        string? colorHex = null,
        Money? priceOverride = null,
        int stockQuantity = 0,
        JsonDocument? attributes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        return new ProductVariant
        {
            ProductId = productId,
            Sku = sku.ToUpperInvariant(),
            Size = size,
            Color = color,
            ColorHex = colorHex,
            PriceOverride = priceOverride,
            StockQuantity = stockQuantity,
            Attributes = attributes,
        };
    }

    public void Update(
        ProductSize size,
        string? color,
        string? colorHex,
        Money? priceOverride,
        int stockQuantity,
        JsonDocument? attributes)
    {
        Size = size;
        Color = color;
        ColorHex = colorHex;
        PriceOverride = priceOverride;
        StockQuantity = stockQuantity;
        Attributes = attributes;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
