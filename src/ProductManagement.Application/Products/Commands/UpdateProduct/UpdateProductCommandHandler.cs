using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Products.Events;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ISlugService slugService,
    IPublisher publisher)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);

        var slug = await slugService.GenerateUniqueSlugAsync(request.Name, request.Id, cancellationToken);
        var price = Money.Of(request.BasePrice, request.Currency);

        product.Update(
            request.Name,
            slug,
            request.Brand,
            request.CategoryId,
            price,
            request.Description,
            request.Attributes);

        await unitOfWork.SaveChangeAsync(cancellationToken);

        await publisher.Publish(new ProductChangedNotification(request.Id), cancellationToken);
    }
}
