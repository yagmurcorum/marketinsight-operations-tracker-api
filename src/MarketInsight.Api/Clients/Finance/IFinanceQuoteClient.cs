using MarketInsight.Api.DTOs.Quotes;

namespace MarketInsight.Api.Clients.Finance;

/// <summary>
/// Defines the contract for retrieving quote data from an external finance API.
/// </summary>
public interface IFinanceQuoteClient
{
    /// <summary>
    /// Gets quote data for the requested financial symbol.
    /// </summary>
    /// <param name="symbol">The requested financial symbol.</param>
    /// <param name="apiKey">The external finance API key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The quote response if quote data is available; otherwise null.</returns>
    Task<QuoteResponse?> GetQuoteAsync(
        string symbol,
        string apiKey,
        CancellationToken cancellationToken = default);
}