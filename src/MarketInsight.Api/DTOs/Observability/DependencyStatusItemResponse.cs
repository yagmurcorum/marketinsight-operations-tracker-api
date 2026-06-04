namespace MarketInsight.Api.DTOs.Observability;

/// <summary>
/// Represents the status of a single application dependency.
/// </summary>
public class DependencyStatusItemResponse
{
    /// <summary>
    /// Dependency name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dependency status such as Healthy, Unhealthy, or NotConfigured.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable dependency status message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional dependency response time in milliseconds.
    /// </summary>
    public long? ResponseTimeMilliseconds { get; set; }
}