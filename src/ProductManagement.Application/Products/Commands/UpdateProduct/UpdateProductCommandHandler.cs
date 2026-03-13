using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);

        var slug = await ResolveSlugAsync(request, cancellationToken);
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
    }

    private async Task<Slug> ResolveSlugAsync(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var baseSlug = Slug.Create(request.Name);
        if (!await productRepository.SlugExistsAsync(baseSlug.Value, request.Id, cancellationToken))
            return baseSlug;

        for (var i = 2; i <= 100; i++)
        {
            var candidate = Slug.Create($"{request.Name}-{i}");
            if (!await productRepository.SlugExistsAsync(candidate.Value, request.Id, cancellationToken))
                return candidate;
        }

        throw new InvalidOperationException($"Could not generate a unique slug for '{request.Name}'.");
    }
}
