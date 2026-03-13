namespace ProductManagement.Application.Products.Dtos;

public sealed record ProductSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Brand,
    Guid CategoryId,
    string CategoryName,
    decimal BasePrice,
    string Currency,
    string Status,
    string? PrimaryImageUrl,
    int VariantCount,
    DateTime CreatedAt);
