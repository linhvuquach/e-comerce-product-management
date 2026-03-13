using ProductManagement.Domain.Common;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Domain.Entities;

public sealed class Category : SoftDeletableEntity
{
    private readonly List<Category> _children = [];
    private readonly List<Product> _products = [];

    private Category() { } // EF Core

    public string Name { get; private set; } = default!;
    public Slug Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public Guid? ParentId { get; private set; }
    public Category? Parent { get; private set; }

    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public static Category Create(string name, Slug slug, string? description = null, Guid? parentId = null, int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Category
        {
            Name = name,
            Slug = slug,
            Description = description,
            ParentId = parentId,
            SortOrder = sortOrder,
        };
    }

    public void Update(string name, Slug slug, string? description, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        Name = name;
        Slug = slug;
        Description = description;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }
}
