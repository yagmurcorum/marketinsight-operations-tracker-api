using MarketInsight.Api.Clients.Finance;
using MarketInsight.Api.DTOs.Quotes;
using MarketInsight.Api.Options;
using Microsoft.Extensions.Options;

namespace MarketInsight.Api.Providers.Quotes;

/// <summary>
/// Provides quote data by using the Finnhub finance API client.
/// </summary>
public class FinnhubQuoteProvider : IQuoteProvider
{
    private readonly IFinanceQuoteClient _financeQuoteClient;
    private readonly FinanceApiOptions _options;
    private readonly ILogger<FinnhubQuoteProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinnhubQuoteProvider"/> class.
    /// </summary>
    /// <param name="financeQuoteClient">The finance quote client.</param>
    /// <param name="options">The finance API configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public FinnhubQuoteProvider(
        IFinanceQuoteClient financeQuoteClient,
        IOptions<FinanceApiOptions> options,
        ILogger<FinnhubQuoteProvider> logger)
    {
        _financeQuoteClient = financeQuoteClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<QuoteResponse?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning(
                "Finance API key is not configured for provider {Provider}.",
                _options.Provider);

            return null;
        }

        return await _financeQuoteClient.GetQuoteAsync(
            symbol,
            _options.ApiKey,
            cancellationToken);
    }
}