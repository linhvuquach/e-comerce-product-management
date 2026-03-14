using FluentAssertions;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Unit.Tests.Domain;

public class MoneyTests
{
    // --- Money.Of ---

    [Fact]
    public void Of_ValidAmountAndCurrency_ReturnsMoney()
    {
        var money = Money.Of(10.50m, "USD");

        money.Amount.Should().Be(10.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Of_UppercasesCurrency()
    {
        var money = Money.Of(5m, "usd");

        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Of_ZeroAmount_IsAllowed()
    {
        var money = Money.Of(0m, "EUR");

        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Of_NegativeAmount_ThrowsArgumentException()
    {
        var act = () => Money.Of(-1m, "USD");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Of_InvalidCurrency_ThrowsArgumentException(string currency)
    {
        var act = () => Money.Of(10m, currency);

        act.Should().Throw<ArgumentException>();
    }

    // --- Money.Add ---

    [Fact]
    public void Add_SameCurrency_ReturnsSummedMoney()
    {
        var a = Money.Of(10m, "USD");
        var b = Money.Of(5m, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(15m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        var usd = Money.Of(10m, "USD");
        var eur = Money.Of(5m, "EUR");

        var act = () => usd.Add(eur);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*USD*EUR*");
    }

    // --- Zero ---

    [Fact]
    public void Zero_HasZeroAmountAndUsdCurrency()
    {
        Money.Zero.Amount.Should().Be(0m);
        Money.Zero.Currency.Should().Be("USD");
    }

    // --- ToString ---

    [Fact]
    public void ToString_FormatsAmountWithCurrency()
    {
        var money = Money.Of(9.99m, "USD");

        money.ToString().Should().Contain("USD")
            .And.Subject.Should().EndWith("USD");
        money.Amount.Should().Be(9.99m);
    }

    // --- Equality (record semantics) ---

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.Of(100m, "GBP");
        var b = Money.Of(100m, "GBP");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentAmount_AreNotEqual()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(200m, "USD");

        a.Should().NotBe(b);
    }
}
