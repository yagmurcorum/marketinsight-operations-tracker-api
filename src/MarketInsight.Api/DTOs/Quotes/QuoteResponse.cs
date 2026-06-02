namespace MarketInsight.Api.DTOs.Quotes;

/// <summary>
/// Represents the internal quote response used by the MarketInsight API.
/// </summary>
public class QuoteResponse
{
    /// <summary>
    /// Financial symbol for the quote.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Current market price.
    /// </summary>
    public decimal CurrentPrice { get; set; }

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
    /// Quote retrieval timestamp in UTC.
    /// </summary>
    public DateTime RetrievedAtUtc { get; set; }

    /// <summary>
    /// Quote data source name.
    /// </summary>
    public string Source { get; set; } = string.Empty;
}