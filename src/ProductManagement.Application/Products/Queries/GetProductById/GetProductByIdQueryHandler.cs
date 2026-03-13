using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Products.Dtos;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.Application.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var (product, xmin) = await productRepository.GetWithVariantsImagesAsync(
            request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        return new GetProductByIdResult(MapToDetail(product), xmin);
    }

    private static ProductDetailDto MapToDetail(Product p) => new(
        p.Id,
        p.Name,
        p.Slug.Value,
        p.Description,
        p.Brand,
        p.CategoryId,
        p.Category?.Name ?? string.Empty,
        p.BasePrice.Amount,
        p.BasePrice.Currency,
        p.Status.ToString(),
        p.Attributes,
        p.Variants.Select(v => new ProductVariantDto(
            v.Id,
            v.Sku,
            v.Size.ToString(),
            v.Color,
            v.ColorHex,
            v.PriceOverride?.Amount,
            v.PriceOverride?.Amount ?? p.BasePrice.Amount,
            v.StockQuantity,
            v.IsActive,
            v.Attributes)).ToList(),
        p.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto(
            i.Id,
            i.CdnUrl,
            i.BlobUrl,
            i.AltText,
            i.SortOrder,
            i.IsPrimary,
            i.VariantId)).ToList(),
        p.CreatedAt,
        p.UpdatedAt);
}
