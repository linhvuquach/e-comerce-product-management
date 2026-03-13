using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Products.Dtos;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Products.Queries.GetProducts;

internal sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductSummaryDto>>
{
    private const int _maxPageSize = 100;

    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, _maxPageSize);
        var page = Math.Max(request.Page, 1);

        var (items, totalCount) = await productRepository.GetPagedAsync(
            page,
            pageSize,
            request.Q,
            request.CategoryId,
            request.Status,
            request.MinPrice,
            request.MaxPrice,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var dtos = items.Select(MapToSummary).ToList();
        return new PagedResult<ProductSummaryDto>(dtos, totalCount, page, pageSize);
    }

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
