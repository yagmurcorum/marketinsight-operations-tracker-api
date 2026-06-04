namespace MarketInsight.Api.DTOs.Quotes;

/// <summary>
/// Represents the response returned after an asynchronous price refresh request is queued.
/// </summary>
public class AsyncPriceRefreshResponse
{
    /// <summary>
    /// Response message describing the async refresh request status.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Symbol accepted for asynchronous price refresh.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Queue status of the asynchronous refresh request.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}