using FluentAssertions;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Unit.Tests.Domain;

public class ProductTests
{
    private static readonly Guid DefaultCategoryId = Guid.NewGuid();
    private static readonly Money DefaultPrice = Money.Of(99.99m, "USD");
    private static readonly Slug DefaultSlug = Slug.Create("test-product");

    private static Product CreateProduct(
        string name = "Test Product",
        string brand = "Test Brand",
        Guid? categoryId = null,
        Money? price = null,
        string? description = null) =>
        Product.Create(
            name,
            DefaultSlug,
            brand,
            categoryId ?? DefaultCategoryId,
            price ?? DefaultPrice,
            description);

    // --- Product.Create ---

    [Fact]
    public void Create_ValidArguments_SetsAllProperties()
    {
        var categoryId = Guid.NewGuid();
        var price = Money.Of(49.99m, "EUR");
        var slug = Slug.Create("cool-jacket");

        var product = Product.Create("Cool Jacket", slug, "Zara", categoryId, price, "A jacket");

        product.Name.Should().Be("Cool Jacket");
        product.Slug.Should().Be(slug);
        product.Brand.Should().Be("Zara");
        product.CategoryId.Should().Be(categoryId);
        product.BasePrice.Should().Be(price);
        product.Description.Should().Be("A jacket");
        product.Status.Should().Be(ProductStatus.Draft);
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_NewProduct_StartsAsDraft()
    {
        var product = CreateProduct();

        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Create_NewProduct_HasEmptyVariantsAndImages()
    {
        var product = CreateProduct();

        product.Variants.Should().BeEmpty();
        product.Images.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyName_ThrowsArgumentException(string name)
    {
        var act = () => CreateProduct(name: name);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyBrand_ThrowsArgumentException(string brand)
    {
        var act = () => CreateProduct(brand: brand);

        act.Should().Throw<ArgumentException>();
    }

    // --- Product.Update ---

    [Fact]
    public void Update_ValidArguments_UpdatesProperties()
    {
        var product = CreateProduct();
        var newSlug = Slug.Create("updated-product");
        var newPrice = Money.Of(199.99m, "USD");
        var newCategoryId = Guid.NewGuid();

        product.Update("Updated Name", newSlug, "New Brand", newCategoryId, newPrice, "New desc", null);

        product.Name.Should().Be("Updated Name");
        product.Slug.Should().Be(newSlug);
        product.Brand.Should().Be("New Brand");
        product.CategoryId.Should().Be(newCategoryId);
        product.BasePrice.Should().Be(newPrice);
        product.Description.Should().Be("New desc");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_EmptyName_ThrowsArgumentException(string name)
    {
        var product = CreateProduct();

        var act = () => product.Update(name, DefaultSlug, "Brand", DefaultCategoryId, DefaultPrice, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_EmptyBrand_ThrowsArgumentException(string brand)
    {
        var product = CreateProduct();

        var act = () => product.Update("Name", DefaultSlug, brand, DefaultCategoryId, DefaultPrice, null, null);

        act.Should().Throw<ArgumentException>();
    }

    // --- Product.TransitionStatus ---

    [Theory]
    [InlineData(ProductStatus.Draft, ProductStatus.Active)]
    [InlineData(ProductStatus.Draft, ProductStatus.Archived)]
    [InlineData(ProductStatus.Active, ProductStatus.Archived)]
    [InlineData(ProductStatus.Active, ProductStatus.Draft)]
    [InlineData(ProductStatus.Archived, ProductStatus.Draft)]
    public void TransitionStatus_AllowedTransitions_UpdatesStatus(ProductStatus from, ProductStatus to)
    {
        var product = CreateProduct();
        if (product.Status != from)
            product.TransitionStatus(from);

        product.TransitionStatus(to);

        product.Status.Should().Be(to);
    }

    [Fact]
    public void TransitionStatus_ArchivedToActive_ThrowsInvalidProductStatusTransitionException()
    {
        var product = CreateProduct();
        product.TransitionStatus(ProductStatus.Archived);

        var act = () => product.TransitionStatus(ProductStatus.Active);

        act.Should().Throw<InvalidProductStatusTransitionException>()
            .WithMessage("*Archived*Active*");
    }

    // --- Product.AddVariant ---

    [Fact]
    public void AddVariant_AddsVariantToCollection()
    {
        var product = CreateProduct();
        var variant = ProductVariant.Create(product.Id, "SKU-001", ProductSize.M);

        product.AddVariant(variant);

        product.Variants.Should().ContainSingle()
            .Which.Sku.Should().Be("SKU-001");
    }

    [Fact]
    public void AddVariant_MultipleVariants_AllPresent()
    {
        var product = CreateProduct();
        var v1 = ProductVariant.Create(product.Id, "SKU-001", ProductSize.S);
        var v2 = ProductVariant.Create(product.Id, "SKU-002", ProductSize.L);

        product.AddVariant(v1);
        product.AddVariant(v2);

        product.Variants.Should().HaveCount(2);
    }

    // --- Product.AddImage / RemoveImage ---

    [Fact]
    public void AddImage_AddsImageToCollection()
    {
        var product = CreateProduct();
        var image = ProductImage.Create(product.Id, "https://example.com/img.jpg");

        product.AddImage(image);

        product.Images.Should().ContainSingle();
    }

    [Fact]
    public void RemoveImage_ExistingImage_RemovesFromCollection()
    {
        var product = CreateProduct();
        var image = ProductImage.Create(product.Id, "https://example.com/img.jpg");
        product.AddImage(image);

        product.RemoveImage(image.Id);

        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void RemoveImage_NonExistentImageId_ThrowsInvalidOperationException()
    {
        var product = CreateProduct();

        var act = () => product.RemoveImage(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // --- SoftDelete (inherited) ---

    [Fact]
    public void SoftDelete_SetsDeletedAtAndIsDeleted()
    {
        var product = CreateProduct();

        product.SoftDelete();

        product.IsDeleted.Should().BeTrue();
        product.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Restore_AfterSoftDelete_ClearsDeletedAt()
    {
        var product = CreateProduct();
        product.SoftDelete();

        product.Restore();

        product.IsDeleted.Should().BeFalse();
        product.DeletedAt.Should().BeNull();
    }
}
