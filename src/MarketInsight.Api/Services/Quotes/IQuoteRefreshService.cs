namespace MarketInsight.Api.Services.Quotes;

/// <summary>
/// Defines the quote refresh use case for active watchlist items.
/// </summary>
public interface IQuoteRefreshService
{
    /// <summary>
    /// Refreshes quote data for an active watchlist symbol.
    /// </summary>
    /// <param name="symbol">The requested financial symbol.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The application-level quote refresh result.</returns>
    Task<QuoteRefreshResult> RefreshQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}