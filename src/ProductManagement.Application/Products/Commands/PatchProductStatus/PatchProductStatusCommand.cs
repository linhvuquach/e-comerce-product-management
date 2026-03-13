using MediatR;

namespace ProductManagement.Application.Products.Commands.PatchProductStatus;

public sealed record PatchProductStatusCommand(Guid Id, string Status) : IRequest;
