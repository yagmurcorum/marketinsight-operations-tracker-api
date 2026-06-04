namespace MarketInsight.Api.Messaging;

/// <summary>
/// Represents a RabbitMQ message for asynchronous price refresh processing.
/// </summary>
public class PriceRefreshMessage
{
    /// <summary>
    /// Normalized symbol requested for price refresh.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// UTC date and time when the refresh request was created.
    /// </summary>
    public DateTime RequestedAtUtc { get; set; }

    /// <summary>
    /// Correlation identifier used to track the async refresh flow.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
}