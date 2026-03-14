namespace ProductManagement.API.Options;

public sealed class RateLimitSettings
{
    public const string Section = "RateLimit";
    public const string WritePolicyName = "write";

    public int WriteRequestsPerMinute { get; init; } = 1000;
    public int QueueLimit { get; init; }
}
