using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Application.Common.Interfaces;

public interface ISlugService
{
    Task<Slug> GenerateUniqueSlugAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
