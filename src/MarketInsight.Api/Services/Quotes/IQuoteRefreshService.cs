using MarketInsight.Api.DTOs.Quotes;

namespace MarketInsight.Api.Services.Quotes;

/// <summary>
/// Defines quote operations for active watchlist items.
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

    /// <summary>
    /// Gets saved price snapshots for an active watchlist symbol.
    /// </summary>
    /// <param name="symbol">The requested financial symbol.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A list of saved price snapshots if the symbol is active; otherwise null.</returns>
    Task<List<PriceSnapshotResponse>?> GetSnapshotsAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}