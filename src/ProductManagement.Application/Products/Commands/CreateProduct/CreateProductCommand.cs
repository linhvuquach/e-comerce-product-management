using System.Text.Json;
using MediatR;

namespace ProductManagement.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Brand,
    Guid CategoryId,
    decimal BasePrice,
    string Currency,
    string? Description,
    JsonDocument? Attributes) : IRequest<Guid>;
