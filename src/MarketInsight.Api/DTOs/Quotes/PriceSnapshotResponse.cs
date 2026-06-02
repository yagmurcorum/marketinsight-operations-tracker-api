namespace MarketInsight.Api.DTOs.Quotes;

/// <summary>
/// Represents a saved price snapshot response.
/// </summary>
public class PriceSnapshotResponse
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
    /// Financial symbol related to the snapshot.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Saved price value.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Currency of the saved price.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Source of the price data.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// UTC date and time when the price data was retrieved.
    /// </summary>
    public DateTime RetrievedAtUtc { get; set; }

    /// <summary>
    /// UTC date and time when the snapshot was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}