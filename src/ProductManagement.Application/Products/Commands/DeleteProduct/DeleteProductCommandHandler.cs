using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Products.Events;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.Application.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);

        await productRepository.DeleteAsync(product, cancellationToken);
        await unitOfWork.SaveChangeAsync(cancellationToken);

        await publisher.Publish(new ProductChangedNotification(request.Id), cancellationToken);
    }
}
