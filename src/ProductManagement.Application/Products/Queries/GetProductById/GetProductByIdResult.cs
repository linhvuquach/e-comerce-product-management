using ProductManagement.Application.Products.Dtos;

namespace ProductManagement.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdResult(ProductDetailDto Product, uint Xmin);
