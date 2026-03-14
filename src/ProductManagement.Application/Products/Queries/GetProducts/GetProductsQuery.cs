using MediatR;
using ProductManagement.Application.Products.Dtos;

namespace ProductManagement.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Q = null,
    Guid? CategoryId = null,
    string? Status = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string[]? Sizes = null,
    string[]? Colors = null,
    string? SortBy = null,
    bool SortDescending = true) : IRequest<PagedResult<ProductSummaryDto>>;
