namespace MarketInsight.Api.DTOs.Quotes;

/// <summary>
/// Represents the response returned after a successful quote refresh operation.
/// </summary>
public class QuoteRefreshResponse
{
    /// <summary>
    /// Financial symbol that was refreshed.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Current market price.
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Currency of the quote.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Price change.
    /// </summary>
    public decimal? Change { get; set; }

    /// <summary>
    /// Price change percentage.
    /// </summary>
    public decimal? PercentChange { get; set; }

    /// <summary>
    /// Highest price of the day.
    /// </summary>
    public decimal? HighPriceOfDay { get; set; }

    /// <summary>
    /// Lowest price of the day.
    /// </summary>
    public decimal? LowPriceOfDay { get; set; }

    /// <summary>
    /// Opening price of the day.
    /// </summary>
    public decimal? OpenPriceOfDay { get; set; }

    /// <summary>
    /// Previous closing price.
    /// </summary>
    public decimal? PreviousClosePrice { get; set; }

    /// <summary>
    /// Indicates whether the quote was returned from Redis cache.
    /// </summary>
    public bool IsFromCache { get; set; }

    /// <summary>
    /// Source of the quote data.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// UTC date and time when the quote data was retrieved.
    /// </summary>
    public DateTime RetrievedAtUtc { get; set; }

    /// <summary>
    /// Identifier of the saved price snapshot.
    /// </summary>
    public int PriceSnapshotId { get; set; }

    /// <summary>
    /// UTC date and time when the price snapshot was created.
    /// </summary>
    public DateTime SnapshotCreatedAtUtc { get; set; }
}