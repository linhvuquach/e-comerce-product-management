using MediatR;

namespace ProductManagement.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<GetProductByIdResult>;
