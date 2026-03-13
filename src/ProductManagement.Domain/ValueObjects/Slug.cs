using System.Text.RegularExpressions;

namespace ProductManagement.Domain.ValueObjects;

public sealed record Slug
{
    public string Value { get; }

    private Slug(string value) => Value = value;

    public static Slug Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Slug input cannot be empty.", nameof(input));
        }

        var slug = input.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-{2,}", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug))
        {
            throw new ArgumentException("Slug cannot be empty after sanitisation.", nameof(input));
        }

        return new Slug(slug);
    }

    public static Slug FromRaw(string raw) => new(raw);

    public override string ToString() => Value;
}
