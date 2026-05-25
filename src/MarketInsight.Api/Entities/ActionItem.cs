namespace MarketInsight.Api.Entities;

/// <summary>
/// Represents an operational follow-up action created after a price alert or important price event.
/// </summary>
public class ActionItem
{
    /// <summary>
    /// Unique identifier of the action item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the related watchlist item.
    /// </summary>
    public int WatchlistItemId { get; set; }

    /// <summary>
    /// Optional identifier of the related price alert.
    /// </summary>
    public int? PriceAlertId { get; set; }

    /// <summary>
    /// Financial symbol related to this action item.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Short title of the action item.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed description of the action item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Current status of the action item, such as Pending or Completed.
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// UTC date and time when the action item was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC date and time when the action item was completed.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Related watchlist item navigation property.
    /// </summary>
    public WatchlistItem? WatchlistItem { get; set; }

    /// <summary>
    /// Related price alert navigation property.
    /// </summary>
    public PriceAlert? PriceAlert { get; set; }
}