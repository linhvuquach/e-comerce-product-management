namespace ProductManagement.API.Options;

public sealed class CorsSettings
{
    public const string Section = "Cors";
    public const string PolicyName = "Frontend";

    public string[] AllowedOrigins { get; init; } = [];
}
