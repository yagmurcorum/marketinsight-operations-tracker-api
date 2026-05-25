namespace MarketInsight.Api.Entities;

/// <summary>
/// Represents a persistent price history record for a tracked financial symbol.
/// </summary>
public class PriceSnapshot
{
    /// <summary>
    /// Unique identifier of the price snapshot.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the related watchlist item.
    /// </summary>
    public int WatchlistItemId { get; set; }

    /// <summary>
    /// Financial symbol related to this price snapshot.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Price value retrieved from the external finance API.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Currency of the retrieved price value.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Source of the retrieved price data.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// UTC date and time when the price data was retrieved.
    /// </summary>
    public DateTime RetrievedAtUtc { get; set; }

    /// <summary>
    /// UTC date and time when the price snapshot record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Related watchlist item navigation property.
    /// </summary>
    public WatchlistItem? WatchlistItem { get; set; }
}