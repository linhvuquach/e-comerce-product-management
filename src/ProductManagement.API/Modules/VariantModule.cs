using Carter;
using MediatR;
using ProductManagement.Application.Variants.Queries.GetVariantsByProduct;

namespace ProductManagement.API.Modules;

// TODO: Expand to full Variant endpoints (T2.7) — currently a placeholder
public sealed class VariantModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products/{productId:guid}/variants")
            .WithTags("Variants");

        group.MapGet("/", async (Guid productId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetVariantsByProductQuery(productId), ct);
            return Results.Ok(result);
        })
        .WithName("GetVariantsByProduct")
        .WithSummary("Get variants for a product")
        .Produces<IReadOnlyList<VariantDto>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
