using MarketInsight.Api.DTOs.Quotes;

namespace MarketInsight.Api.Services.Cache;

/// <summary>
/// Defines cache operations for quote data.
/// </summary>
public interface IQuoteCacheService
{
    /// <summary>
    /// Gets cached quote data for the requested symbol.
    /// </summary>
    /// <param name="symbol">The requested financial symbol.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The cached quote response if available; otherwise null.</returns>
    Task<QuoteResponse?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores quote data in cache for the requested symbol.
    /// </summary>
    /// <param name="symbol">The requested financial symbol.</param>
    /// <param name="quote">The quote response to cache.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task SetQuoteAsync(
        string symbol,
        QuoteResponse quote,
        CancellationToken cancellationToken = default);
}