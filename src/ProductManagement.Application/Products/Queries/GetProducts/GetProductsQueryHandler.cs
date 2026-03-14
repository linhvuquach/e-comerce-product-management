using MediatR;
using ProductManagement.Application.Common.Cache;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Common.Models;
using ProductManagement.Application.Products.Dtos;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Products.Queries.GetProducts;

internal sealed class GetProductsQueryHandler(
    IProductRepository productRepository,
    ICacheService cache)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductSummaryDto>>
{
    private static readonly TimeSpan _listCacheTtl = TimeSpan.FromMinutes(5);

    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = new PaginationParams { Page = request.Page, PageSize = request.PageSize }.PageSizeClamped;
        var page = new PaginationParams { Page = request.Page, PageSize = request.PageSize }.PageClamped;

        var cacheKey = BuildCacheKey(page, pageSize, request);

        return await cache.GetOrSetAsync(
            cacheKey,
            () => FetchFromDbAsync(page, pageSize, request, cancellationToken),
            _listCacheTtl,
            cancellationToken);
    }

    private async Task<PagedResult<ProductSummaryDto>> FetchFromDbAsync(
        int page, int pageSize, GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await productRepository.GetPagedAsync(
            page,
            pageSize,
            request.Q,
            request.CategoryId,
            request.Status,
            request.MinPrice,
            request.MaxPrice,
            request.Sizes,
            request.Colors,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var dtos = items.Select(MapToSummary).ToList();
        return new PagedResult<ProductSummaryDto>(dtos, totalCount, page, pageSize);
    }

    private static string BuildCacheKey(int page, int pageSize, GetProductsQuery r) =>
        $"{ProductCacheKeys.ListKeyPrefix}p={page}&ps={pageSize}&q={r.Q}&cat={r.CategoryId}&s={r.Status}" +
        $"&min={r.MinPrice}&max={r.MaxPrice}" +
        $"&sizes={string.Join(',', r.Sizes ?? [])}&colors={string.Join(',', r.Colors ?? [])}" +
        $"&sort={r.SortBy}&desc={r.SortDescending}";

    private static ProductSummaryDto MapToSummary(Product p) => new(
        p.Id,
        p.Name,
        p.Slug.Value,
        p.Brand,
        p.CategoryId,
        p.Category?.Name ?? string.Empty,
        p.BasePrice.Amount,
        p.BasePrice.Currency,
        p.Status.ToString(),
        p.Images.FirstOrDefault(i => i.IsPrimary)?.CdnUrl
            ?? p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.CdnUrl,
        p.Variants.Count,
        p.CreatedAt);
}
