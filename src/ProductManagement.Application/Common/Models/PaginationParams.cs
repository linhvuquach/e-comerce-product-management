namespace ProductManagement.Application.Common.Models;

public sealed record PaginationParams
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;

    public int Page {get; init;} = 1;
    public int PageSize {get; init;} = DefaultPageSize;

    public int PageClamped => Math.Max(Page, 1);
    public int PageSizeClamped => Math.Min(Math.Max(PageSize, 1), MaxPageSize);
}
