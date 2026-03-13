using MediatR;

namespace ProductManagement.Application.Variants.Queries.GetVariantsByProduct;

// TODO: Expand to full Variant CQRS (T2.6)
public sealed record GetVariantsByProductQuery(Guid ProductId) : IRequest<IReadOnlyList<VariantDto>>;

public sealed record VariantDto(
    Guid Id,
    Guid ProductId,
    string Sku,
    string Size,
    string? Color,
    string? ColorHex,
    decimal? PriceOverride,
    string? PriceOverrideCurrency,
    int StockQuantity,
    bool IsActive);
