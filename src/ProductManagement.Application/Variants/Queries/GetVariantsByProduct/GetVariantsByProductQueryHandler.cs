using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.Application.Variants.Queries.GetVariantsByProduct;

// TODO: Expand to full Variant CQRS (T2.6) — currently a placeholder
internal sealed class GetVariantsByProductQueryHandler(
    IProductRepository productRepository)
    : IRequestHandler<GetVariantsByProductQuery, IReadOnlyList<VariantDto>>
{
    public async Task<IReadOnlyList<VariantDto>> Handle(
        GetVariantsByProductQuery request,
        CancellationToken cancellationToken)
    {
        var (product, _) = await productRepository.GetWithVariantsImagesAsync(request.ProductId, cancellationToken);
        if (product is null)
            throw new ProductNotFoundException(request.ProductId);

        return product.Variants
            .Select(v => new VariantDto(
                v.Id,
                v.ProductId,
                v.Sku,
                v.Size.ToString(),
                v.Color,
                v.ColorHex,
                v.PriceOverride?.Amount,
                v.PriceOverride?.Currency,
                v.StockQuantity,
                v.IsActive))
            .ToList();
    }
}
