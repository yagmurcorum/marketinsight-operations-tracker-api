using System.Text.Json.Serialization;

namespace MarketInsight.Api.Clients.Finance;

/// <summary>
/// Represents the raw quote response returned by the Finnhub quote API.
/// </summary>
public class FinnhubQuoteResponse
{
    /// <summary>
    /// Current price.
    /// </summary>
    [JsonPropertyName("c")]
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Price change.
    /// </summary>
    [JsonPropertyName("d")]
    public decimal? Change { get; set; }

    /// <summary>
    /// Percent change.
    /// </summary>
    [JsonPropertyName("dp")]
    public decimal? PercentChange { get; set; }

    /// <summary>
    /// High price of the day.
    /// </summary>
    [JsonPropertyName("h")]
    public decimal? HighPriceOfDay { get; set; }

    /// <summary>
    /// Low price of the day.
    /// </summary>
    [JsonPropertyName("l")]
    public decimal? LowPriceOfDay { get; set; }

    /// <summary>
    /// Open price of the day.
    /// </summary>
    [JsonPropertyName("o")]
    public decimal? OpenPriceOfDay { get; set; }

    /// <summary>
    /// Previous close price.
    /// </summary>
    [JsonPropertyName("pc")]
    public decimal? PreviousClosePrice { get; set; }

    /// <summary>
    /// Unix timestamp returned by the external API.
    /// </summary>
    [JsonPropertyName("t")]
    public long UnixTimestamp { get; set; }
}