using System.Text.Json;

namespace ProductManagement.Application.Products.Dtos;

public sealed record ProductDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Brand,
    Guid CategoryId,
    string CategoryName,
    decimal BasePrice,
    string Currency,
    string Status,
    JsonDocument? Attributes,
    IReadOnlyList<ProductVariantDto> Variants,
    IReadOnlyList<ProductImageDto> Images,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
