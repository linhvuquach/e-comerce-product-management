namespace ProductManagement.Domain.Exceptions;

public sealed class ProductNotFoundException(Guid productId)
    : DomainException($"Product with ID '{productId}' was not found.");
