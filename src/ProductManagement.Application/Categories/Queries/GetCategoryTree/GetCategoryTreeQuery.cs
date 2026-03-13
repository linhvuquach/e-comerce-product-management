using MediatR;

namespace ProductManagement.Application.Categories.Queries.GetCategoryTree;

public sealed record GetCategoryTreeQuery : IRequest<IReadOnlyList<CategoryDto>>;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    int SortOrder,
    Guid? ParentId,
    IReadOnlyList<CategoryDto> Children);
