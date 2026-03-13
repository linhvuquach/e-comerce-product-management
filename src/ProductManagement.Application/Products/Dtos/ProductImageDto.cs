namespace ProductManagement.Application.Products.Dtos;

public sealed record ProductImageDto(
    Guid Id,
    string? CdnUrl,
    string BlobUrl,
    string? AltText,
    int SortOrder,
    bool IsPrimary,
    Guid? VariantId);
