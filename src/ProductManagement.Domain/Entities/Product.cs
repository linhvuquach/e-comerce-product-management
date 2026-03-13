using System.Text.Json;
using ProductManagement.Domain.Common;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Domain.Entities;

public sealed class Product : SoftDeletableEntity
{
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductImage> _images = [];

    private Product() { } // EF Core

    public string Name { get; private set; } = default!;
    public Slug Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Brand { get; private set; } = default!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;
    public Money BasePrice { get; private set; } = Money.Zero;
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    public JsonDocument? Attributes { get; private set; }

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    public static Product Create(
        string name,
        Slug slug,
        string brand,
        Guid categoryId,
        Money basePrice,
        string? description = null,
        JsonDocument? attributes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(brand);

        return new Product
        {
            Name = name,
            Slug = slug,
            Brand = brand,
            CategoryId = categoryId,
            BasePrice = basePrice,
            Description = description,
            Attributes = attributes,
            Status = ProductStatus.Draft,
        };
    }

    public void Update(
        string name,
        Slug slug,
        string brand,
        Guid categoryId,
        Money basePrice,
        string? description,
        JsonDocument? attributes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(brand);

        Name = name;
        Slug = slug;
        Brand = brand;
        CategoryId = categoryId;
        BasePrice = basePrice;
        Description = description;
        Attributes = attributes;
        SetUpdatedAt();
    }

    public void TransitionStatus(ProductStatus newStatus)
    {
        // Disallow re-activating archived products
        if (Status == ProductStatus.Archived && newStatus == ProductStatus.Active)
        {
            throw new InvalidProductStatusTransitionException(Status, newStatus);
        }

        Status = newStatus;
        SetUpdatedAt();
    }

    public void AddVariant(ProductVariant variant) => _variants.Add(variant);

    public void AddImage(ProductImage image) => _images.Add(image);

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new InvalidOperationException($"Image '{imageId}' not found on product");

        _images.Remove(image);
    }
}
