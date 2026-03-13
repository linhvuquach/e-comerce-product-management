using Carter;
using MediatR;
using ProductManagement.Application.Categories.Queries.GetCategoryTree;

namespace ProductManagement.API.Modules;

public sealed class CategoryModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCategoryTreeQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetCategoryTree")
        .WithSummary("Get category tree")
        .Produces<IReadOnlyList<CategoryDto>>();
    }
}
