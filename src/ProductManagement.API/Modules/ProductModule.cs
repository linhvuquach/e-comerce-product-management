using System.Text.Json;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Options;
using ProductManagement.Application.Products.Commands.CreateProduct;
using ProductManagement.Application.Products.Commands.DeleteProduct;
using ProductManagement.Application.Products.Commands.PatchProductStatus;
using ProductManagement.Application.Products.Commands.UpdateProduct;
using ProductManagement.Application.Products.Dtos;
using ProductManagement.Application.Products.Queries.GetProductById;
using ProductManagement.Application.Products.Queries.GetProducts;

namespace ProductManagement.API.Modules;

public sealed class ProductModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products");

        group.MapGet("/", GetProductsAsync)
            .WithName("GetProducts")
            .WithSummary("Get paginated product list")
            .Produces<PagedResult<ProductSummaryDto>>();

        group.MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Get product by ID")
            .Produces<ProductDetailDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireRateLimiting(RateLimitSettings.WritePolicyName)
            .RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateProductAsync)
            .WithName("UpdateProduct")
            .WithSummary("Full update — requires If-Match ETag header")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status428PreconditionRequired)
            .RequireRateLimiting(RateLimitSettings.WritePolicyName)
            .RequireAuthorization();

        group.MapPatch("/{id:guid}/status", PatchProductStatusAsync)
            .WithName("PatchProductStatus")
            .WithSummary("Update product status (Draft / Active / Archived)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .RequireRateLimiting(RateLimitSettings.WritePolicyName)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteProductAsync)
            .WithName("DeleteProduct")
            .WithSummary("Soft-delete a product")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireRateLimiting(RateLimitSettings.WritePolicyName)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetProductsAsync(
        ISender sender,
        int page = 1,
        int pageSize = 20,
        string? q = null,
        Guid? categoryId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        [FromQuery] string[]? sizes = null,
        [FromQuery] string[]? colors = null,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetProductsQuery(page, pageSize, q, categoryId, status, minPrice, maxPrice, sizes, colors, sortBy, sortDescending), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id, ISender sender, HttpContext httpContext, CancellationToken ct)
    {
        var result = await sender.Send(new GetProductByIdQuery(id), ct);
        httpContext.Response.Headers.ETag = EncodeETag(result.Xmin);
        return Results.Ok(result.Product);
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request, ISender sender, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Brand,
            request.CategoryId,
            request.BasePrice,
            request.Currency,
            request.Description,
            request.Attributes);

        var id = await sender.Send(command, ct);
        return Results.CreatedAtRoute("GetProductById", new { id }, id);
    }

    private static async Task<IResult> UpdateProductAsync(
        Guid id, UpdateProductRequest request, ISender sender, HttpContext httpContext, CancellationToken ct)
    {
        var ifMatch = httpContext.Request.Headers.IfMatch.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(ifMatch))
            return Results.Problem(
                title: "Precondition Required",
                detail: "The If-Match header with the current ETag is required for updates.",
                statusCode: StatusCodes.Status428PreconditionRequired,
                instance: httpContext.Request.Path);

        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Brand,
            request.CategoryId,
            request.BasePrice,
            request.Currency,
            request.Description,
            request.Attributes);

        await sender.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> PatchProductStatusAsync(
        Guid id, PatchStatusRequest request, ISender sender, CancellationToken ct)
    {
        await sender.Send(new PatchProductStatusCommand(id, request.Status), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteProductAsync(
        Guid id, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DeleteProductCommand(id), ct);
        return Results.NoContent();
    }

    private static string EncodeETag(uint xmin) =>
        $"\"{Convert.ToBase64String(BitConverter.GetBytes(xmin))}\"";
}

// Request DTOs
public sealed record CreateProductRequest(
    string Name,
    string Brand,
    Guid CategoryId,
    decimal BasePrice,
    string Currency = "USD",
    string? Description = null,
    JsonDocument? Attributes = null);

public sealed record UpdateProductRequest(
    string Name,
    string Brand,
    Guid CategoryId,
    decimal BasePrice,
    string Currency = "USD",
    string? Description = null,
    JsonDocument? Attributes = null);

public sealed record PatchStatusRequest(string Status);
