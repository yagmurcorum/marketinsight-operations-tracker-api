namespace MarketInsight.Api.DTOs.Observability;

/// <summary>
/// Represents a beginner-friendly summary of the project's observability capabilities.
/// </summary>
public class ObservabilitySummaryResponse
{
    /// <summary>
    /// Application name.
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Current application environment.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// UTC time when the summary was generated.
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; }

    /// <summary>
    /// Short explanation of the observability purpose.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Available observability endpoints.
    /// </summary>
    public List<string> Endpoints { get; set; } = new();

    /// <summary>
    /// Implemented observability features.
    /// </summary>
    public List<string> ObservabilityFeatures { get; set; } = new();
}