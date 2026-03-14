using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Application.Common.Services;

internal sealed class SlugService(IProductRepository productRepository) : ISlugService
{
    public async Task<Slug> GenerateUniqueSlugAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var baseSlug = Slug.Create(name);
        if (!await productRepository.SlugExistsAsync(baseSlug.Value, excludeId, cancellationToken))
            return baseSlug;

        for (var i = 2; i <= 100; i++)
        {
            var candidate = Slug.Create($"{name}-{i}");
            if (!await productRepository.SlugExistsAsync(candidate.Value, excludeId, cancellationToken))
                return candidate;
        }

        throw new InvalidOperationException($"Could not generate a unique slug for '{name}'.");
    }
}
