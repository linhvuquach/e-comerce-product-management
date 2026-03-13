using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(ProductManagementDbContext context)
    : EfRepository<Product>(context), IProductRepository
{
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Slug.Value == slug, cancellationToken);

    public async Task<(Product? Product, uint Xmin)> GetWithVariantsImagesAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var product = await DbSet
            .AsSplitQuery()
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null) return (null, 0);

        var xmin = Context.Entry(product).Property<uint>("xmin").CurrentValue;
        return (product, xmin);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.Slug.Value == slug);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? q = null,
        Guid? categoryId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var lower = q.ToLowerInvariant();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{lower}%") ||
                EF.Functions.ILike(p.Brand, $"%{lower}%"));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<Domain.Enums.ProductStatus>(status, true, out var parsedStatus))
            query = query.Where(p => p.Status == parsedStatus);

        if (minPrice.HasValue)
            query = query.Where(p => p.BasePrice.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.BasePrice.Amount <= maxPrice.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = (sortBy?.ToLowerInvariant(), sortDescending) switch
        {
            ("name",     true)  => query.OrderByDescending(p => p.Name),
            ("name",     false) => query.OrderBy(p => p.Name),
            ("price",    true)  => query.OrderByDescending(p => p.BasePrice.Amount),
            ("price",    false) => query.OrderBy(p => p.BasePrice.Amount),
            (_,          true)  => query.OrderByDescending(p => p.CreatedAt),
            (_,          false) => query.OrderBy(p => p.CreatedAt),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
