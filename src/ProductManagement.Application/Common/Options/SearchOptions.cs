namespace ProductManagement.Application.Common.Options;

public sealed class SearchOptions
{
    public const string Section = "Search";

    public double TriggramThreshold { get; init; } = 0.3;
}
