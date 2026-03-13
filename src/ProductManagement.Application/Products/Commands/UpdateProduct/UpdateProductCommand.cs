using System.Text.Json;
using MediatR;

namespace ProductManagement.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Brand,
    Guid CategoryId,
    decimal BasePrice,
    string Currency,
    string? Description,
    JsonDocument? Attributes) : IRequest;
