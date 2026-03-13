using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.Application.Products.Commands.PatchProductStatus;

internal sealed class PatchProductStatusCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PatchProductStatusCommand>
{
    public async Task Handle(PatchProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);

        var newStatus = Enum.Parse<ProductStatus>(request.Status, ignoreCase: true);
        product.TransitionStatus(newStatus);

        await unitOfWork.SaveChangeAsync(cancellationToken);
    }
}
