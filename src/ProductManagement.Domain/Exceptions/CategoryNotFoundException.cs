namespace ProductManagement.Domain.Exceptions;

public sealed class CategoryNotFoundException(Guid categoryId)
    : DomainException($"Category with ID '{categoryId}' was not found.");
