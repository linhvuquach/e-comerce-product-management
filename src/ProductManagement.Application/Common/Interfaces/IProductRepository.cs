using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(Product? Product, uint Xmin)> GetWithVariantsImagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
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
        CancellationToken cancellationToken = default);
}
