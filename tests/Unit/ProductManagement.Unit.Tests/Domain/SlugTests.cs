using FluentAssertions;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Unit.Tests.Domain;

public class SlugTests
{
    // --- Slug.Create ---

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Nike Air Max", "nike-air-max")]
    [InlineData("  Trim Spaces  ", "trim-spaces")]
    [InlineData("Multiple   Spaces", "multiple-spaces")]
    [InlineData("Special!@#Chars", "specialchars")]
    [InlineData("already-slug", "already-slug")]
    [InlineData("Mixed Case ABC", "mixed-case-abc")]
    public void Create_ValidInput_ReturnsExpectedSlug(string input, string expected)
    {
        var slug = Slug.Create(input);

        slug.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_CollapsesDuplicateDashes()
    {
        var slug = Slug.Create("hello---world");

        slug.Value.Should().Be("hello-world");
    }

    [Fact]
    public void Create_TrimsLeadingAndTrailingDashes()
    {
        var slug = Slug.Create("-hello-");

        slug.Value.Should().Be("hello");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsArgumentException(string input)
    {
        var act = () => Slug.Create(input);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_OnlySpecialChars_ThrowsArgumentException()
    {
        var act = () => Slug.Create("!@#$%");

        act.Should().Throw<ArgumentException>();
    }

    // --- Slug.FromRaw ---

    [Fact]
    public void FromRaw_ReturnsSlugWithExactValue()
    {
        var slug = Slug.FromRaw("raw-slug-value");

        slug.Value.Should().Be("raw-slug-value");
    }

    // --- ToString ---

    [Fact]
    public void ToString_ReturnsSlugValue()
    {
        var slug = Slug.Create("Nike Shoes");

        slug.ToString().Should().Be("nike-shoes");
    }

    // --- Equality (record semantics) ---

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Slug.Create("test-product");
        var b = Slug.Create("test-product");

        a.Should().Be(b);
    }
}
