using System.Text.Json;

namespace ProductManagement.Application.Products.Dtos;

public sealed record ProductVariantDto(
    Guid Id,
    string Sku,
    string Size,
    string? Color,
    string? ColorHex,
    decimal? PriceOverride,
    decimal EffectivePrice,
    int StockQuantity,
    bool IsActive,
    JsonDocument? Attributes);
