using ProductManagement.Domain.Enums;

namespace ProductManagement.Domain.Exceptions;

public sealed class InvalidProductStatusTransitionException(ProductStatus from , ProductStatus to)
    : DomainException($"Cannot transition product status from '{from}' to '{to}'");