using FluentAssertions;
using ProductManagement.Application.Products.Commands.CreateProduct;

namespace ProductManagement.Unit.Tests.Application;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    private static CreateProductCommand ValidCommand() =>
        new("Product Name", "Brand Name", Guid.NewGuid(), 10m, "USD", null, null);

    [Fact]
    public void Validate_ValidCommand_ReturnsSuccess()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyName_FailsValidation(string name)
    {
        var command = ValidCommand() with { Name = name };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_NameExceeds200Chars_FailsValidation()
    {
        var command = ValidCommand() with { Name = new string('A', 201) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyBrand_FailsValidation(string brand)
    {
        var command = ValidCommand() with { Brand = brand };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Brand));
    }

    [Fact]
    public void Validate_BrandExceeds100Chars_FailsValidation()
    {
        var command = ValidCommand() with { Brand = new string('B', 101) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Brand));
    }

    [Fact]
    public void Validate_EmptyCategoryId_FailsValidation()
    {
        var command = ValidCommand() with { CategoryId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CategoryId));
    }

    [Fact]
    public void Validate_NegativeBasePrice_FailsValidation()
    {
        var command = ValidCommand() with { BasePrice = -0.01m };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.BasePrice));
    }

    [Fact]
    public void Validate_ZeroBasePrice_IsAllowed()
    {
        var command = ValidCommand() with { BasePrice = 0m };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Validate_InvalidCurrency_FailsValidation(string currency)
    {
        var command = ValidCommand() with { Currency = currency };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Currency));
    }

    [Fact]
    public void Validate_ExactlyThreeLetterCurrency_IsValid()
    {
        var command = ValidCommand() with { Currency = "EUR" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
