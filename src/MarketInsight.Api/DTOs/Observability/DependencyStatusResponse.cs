namespace MarketInsight.Api.DTOs.Observability;

/// <summary>
/// Represents the dependency status summary of the application.
/// </summary>
public class DependencyStatusResponse
{
    /// <summary>
    /// Overall dependency status.
    /// </summary>
    public string OverallStatus { get; set; } = string.Empty;

    /// <summary>
    /// UTC time when dependency checks were completed.
    /// </summary>
    public DateTime CheckedAtUtc { get; set; }

    /// <summary>
    /// Individual dependency status results.
    /// </summary>
    public List<DependencyStatusItemResponse> Dependencies { get; set; } = new();
}