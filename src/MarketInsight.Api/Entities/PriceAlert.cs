namespace MarketInsight.Api.Entities;

///  <summary>
/// Represents a price alert rule defined for a tracked financial symbol.
/// </summary>
public class PriceAlert
{
    /// <summary>
    /// Unique identifier of the price alert.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the related watchlist item.
    /// </summary>
    public int WatchlistItemId { get; set; }

    /// <summary>
    /// Financial symbol related to this price alert.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Alert condition type , such as Above or Below.
    /// </summary>
    public string ConditionType { get; set; } = string.Empty;

    /// <summary>
    /// Target price value used when evaluating the alert condition.
    /// </summary>
    public decimal TargetPrice { get; set; }

    /// <summary>
    /// Indicates whether the price alert is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC date and time when the price alert was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC date and time when the price alert was last triggered.
    /// </summary>
    public DateTime? LastTriggeredAtUtc { get; set; }

    /// <summary>
    /// Related watchlist item navigation property.
    /// </summary>
    public WatchlistItem? WatchlistItem { get; set; }
}
