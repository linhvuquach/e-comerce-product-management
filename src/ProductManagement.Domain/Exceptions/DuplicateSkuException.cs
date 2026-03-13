namespace ProductManagement.Domain.Exceptions;

public sealed class DuplicateSkuException(string sku)
    : DomainException($"A variant with SKU '{sku}' already exists.");
