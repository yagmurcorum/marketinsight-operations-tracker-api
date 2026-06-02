using MarketInsight.Api.DTOs.Quotes;

namespace MarketInsight.Api.Providers.Quotes;

/// <summary>
/// Defines the contract for retrieving quote data through a finance provider abstraction.
/// </summary>
public interface IQuoteProvider
{
    /// <summary>
    /// Gets quote data for the requested financial symbol.
    /// </summary>
    /// <param name="symbol">The requested financial symbol.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The quote response if quote data is available; otherwise null.</returns>
    Task<QuoteResponse?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}