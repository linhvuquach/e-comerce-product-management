using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NpgsqlTypes;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Common.Options;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(
    ProductManagementDbContext context,
    IOptions<SearchOptions> searchOptions)
    : EfRepository<Product>(context), IProductRepository
{
    private readonly double _triggramThreshold = searchOptions.Value.TriggramThreshold;

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
        string[]? sizes = null,
        string[]? colors = null,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .AsQueryable();

        // Full-text + trigram hybrid search
        if (!string.IsNullOrWhiteSpace(q))
        {
            var threshold = (float)_triggramThreshold;
            query = query.Where(p =>
                EF.Property<NpgsqlTsVector>(p, "SearchVector")
                    .Matches(EF.Functions.WebSearchToTsQuery("english", q)) ||
                EF.Functions.TrigramsSimilarity(p.Name, q) > threshold ||
                EF.Functions.TrigramsSimilarity(p.Brand, q) > threshold);
        }

        // Category subtree filter (includes all descendant categories)
        if (categoryId.HasValue)
        {
            var descendantIds = await GetCategoryDescendantIdsAsync(categoryId.Value, cancellationToken);
            query = query.Where(p => descendantIds.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<Domain.Enums.ProductStatus>(status, true, out var parsedStatus))
            query = query.Where(p => p.Status == parsedStatus);

        if (minPrice.HasValue)
            query = query.Where(p => p.BasePrice.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.BasePrice.Amount <= maxPrice.Value);

        if (sizes?.Length > 0)
            query = query.Where(p => p.Variants.Any(v => v.IsActive && sizes.Contains(v.Size.ToString())));

        if (colors?.Length > 0)
            query = query.Where(p => p.Variants.Any(v => v.IsActive && v.Color != null && colors.Contains(v.Color)));

        var totalCount = await query.CountAsync(cancellationToken);

        // Sort: relevance-first when searching without explicit sortBy; otherwise column sort
        if (!string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(sortBy))
        {
            query = query.OrderByDescending(p =>
                EF.Property<NpgsqlTsVector>(p, "SearchVector")
                    .RankCoverDensity(EF.Functions.WebSearchToTsQuery("english", q)));
        }
        else
        {
            query = (sortBy?.ToLowerInvariant(), sortDescending) switch
            {
                ("name",  true)  => query.OrderByDescending(p => p.Name),
                ("name",  false) => query.OrderBy(p => p.Name),
                ("price", true)  => query.OrderByDescending(p => p.BasePrice.Amount),
                ("price", false) => query.OrderBy(p => p.BasePrice.Amount),
                (_,       true)  => query.OrderByDescending(p => p.CreatedAt),
                (_,       false) => query.OrderBy(p => p.CreatedAt),
            };
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // BFS over in-memory category tree to get self + all descendant IDs
    private async Task<List<Guid>> GetCategoryDescendantIdsAsync(Guid rootId, CancellationToken cancellationToken)
    {
        var allCategories = await Context.Categories
            .Select(c => new { c.Id, c.ParentId })
            .ToListAsync(cancellationToken);

        var result = new List<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var child in allCategories.Where(c => c.ParentId == current))
                queue.Enqueue(child.Id);
        }

        return result;
    }
}
