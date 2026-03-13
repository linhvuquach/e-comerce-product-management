using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(ProductManagementDbContext context)
    : EfRepository<Category>(context), ICategoryRepository
{
    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.Slug.Value == slug, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.Slug.Value == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        await Context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
}
