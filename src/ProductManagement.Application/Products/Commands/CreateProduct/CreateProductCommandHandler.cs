using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);
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

        return product.Id;
    }

    private async Task<Slug> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = Slug.Create(name);
        if (!await productRepository.SlugExistsAsync(baseSlug.Value, cancellationToken: cancellationToken))
            return baseSlug;

        for (var i = 2; i <= 100; i++)
        {
            var candidate = Slug.Create($"{name}-{i}");
            if (!await productRepository.SlugExistsAsync(candidate.Value, cancellationToken: cancellationToken))
                return candidate;
        }

        throw new InvalidOperationException($"Could not generate a unique slug for '{name}'.");
    }
}
