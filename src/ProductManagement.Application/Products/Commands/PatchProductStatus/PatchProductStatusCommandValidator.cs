using FluentValidation;
using ProductManagement.Domain.Enums;

namespace ProductManagement.Application.Products.Commands.PatchProductStatus;

public sealed class PatchProductStatusCommandValidator : AbstractValidator<PatchProductStatusCommand>
{
    private static readonly string[] ValidStatuses =
        Enum.GetNames<ProductStatus>();

    public PatchProductStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}
