namespace MarketInsight.Api.Options;

/// <summary>
/// Represents configuration values used for the external finance API integration.
/// </summary>
public class FinanceApiOptions
{
    /// <summary>
    /// Base URL of the external finance API.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API key used to authenticate with the external finance API.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Selected finance API provider name.
    /// </summary>
    public string Provider { get; set; } = "Finnhub";
}