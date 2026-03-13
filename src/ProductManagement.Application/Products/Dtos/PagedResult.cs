namespace ProductManagement.Application.Products.Dtos;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Data,
    int TotalItems,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}