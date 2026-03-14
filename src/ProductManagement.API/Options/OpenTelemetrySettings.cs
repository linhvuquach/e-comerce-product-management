namespace ProductManagement.API.Options;

public sealed class OpenTelemetrySettings
{
    public const string Section = "OpenTelemetry";

    public string ServiceName { get; init; } = "ProductManagement.API";
    public string? OtlpEndpoint { get; init; }
    public bool ConsoleExporter { get; init; }
}
