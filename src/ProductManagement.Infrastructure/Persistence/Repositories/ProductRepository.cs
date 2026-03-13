using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enums;

namespace ProductManagement.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(ProductManagementDbContext context)
    : EfRepository<Product>(context), IProductRepository
{
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Slug.Value == slug, cancellationToken);

    public async Task<Product?> GetWithVariantsAndImagesAsync(Guid id, CancellationToken cancellationToken = default) =>
            await DbSet
                .AsSplitQuery()
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.Slug.Value == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        Guid? categoryId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{s}%") ||
                EF.Functions.ILike(p.Brand, $"%{s}%"));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.ProductStatus>(status, out var parsedStatus))
        {
            query = query.Where(p => p.Status == parsedStatus);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice.Amount >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice.Amount <= maxPrice.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
