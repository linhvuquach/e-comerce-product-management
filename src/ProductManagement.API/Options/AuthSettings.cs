namespace ProductManagement.API.Options;

public sealed class AuthSettings
{
    public const string Section = "Auth";

    public bool Enabled { get; init; }
    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}
