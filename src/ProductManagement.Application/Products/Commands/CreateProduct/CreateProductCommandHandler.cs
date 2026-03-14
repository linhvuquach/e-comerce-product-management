using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Products.Events;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ISlugService slugService,
    IPublisher publisher)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var slug = await slugService.GenerateUniqueSlugAsync(request.Name, cancellationToken: cancellationToken);
        var price = Money.Of(request.BasePrice, request.Currency);

        var product = Product.Create(
            request.Name,
            slug,
            request.Brand,
            request.CategoryId,
            price,
            request.Description,
            request.Attributes);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangeAsync(cancellationToken);

        await publisher.Publish(new ProductChangedNotification(product.Id), cancellationToken);

        return product.Id;
    }
}
