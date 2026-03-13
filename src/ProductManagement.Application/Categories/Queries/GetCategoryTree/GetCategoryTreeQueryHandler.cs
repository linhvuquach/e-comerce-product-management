using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Categories.Queries.GetCategoryTree;

internal sealed class GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoryTreeQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        var roots = await categoryRepository.GetTreeAsync(cancellationToken);
        return roots.Select(MapToDto).ToList();
    }

    private static CategoryDto MapToDto(Category category) => new(
        category.Id,
        category.Name,
        category.Slug.Value,
        category.Description,
        category.SortOrder,
        category.ParentId,
        category.Children.Select(MapToDto).ToList());
}
