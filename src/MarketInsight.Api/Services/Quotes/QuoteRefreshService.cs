using MarketInsight.Api.DTOs.Quotes;
using MarketInsight.Api.Entities;
using MarketInsight.Api.Providers.Quotes;
using MarketInsight.Api.Repositories;
using MarketInsight.Api.Services.Cache;

namespace MarketInsight.Api.Services.Quotes;

/// <summary>
/// Handles quote refresh operations for active watchlist items.
/// </summary>
public class QuoteRefreshService : IQuoteRefreshService
{
    private const string DefaultCurrency = "USD";

    private readonly IWatchlistItemRepository _watchlistItemRepository;
    private readonly IQuoteCacheService _quoteCacheService;
    private readonly IQuoteProvider _quoteProvider;
    private readonly IPriceSnapshotRepository _priceSnapshotRepository;
    private readonly ILogger<QuoteRefreshService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteRefreshService"/> class.
    /// </summary>
    /// <param name="watchlistItemRepository">The watchlist item repository.</param>
    /// <param name="quoteCacheService">The quote cache service.</param>
    /// <param name="quoteProvider">The quote provider.</param>
    /// <param name="priceSnapshotRepository">The price snapshot repository.</param>
    /// <param name="logger">The logger instance.</param>
    public QuoteRefreshService(
        IWatchlistItemRepository watchlistItemRepository,
        IQuoteCacheService quoteCacheService,
        IQuoteProvider quoteProvider,
        IPriceSnapshotRepository priceSnapshotRepository,
        ILogger<QuoteRefreshService> logger)
    {
        _watchlistItemRepository = watchlistItemRepository;
        _quoteCacheService = quoteCacheService;
        _quoteProvider = quoteProvider;
        _priceSnapshotRepository = priceSnapshotRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<QuoteRefreshResult> RefreshQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return QuoteRefreshResult.NotFound(string.Empty);
        }

        var normalizedSymbol = NormalizeSymbol(symbol);

        _logger.LogInformation(
            "Price refresh requested for symbol {Symbol}.",
            normalizedSymbol);

        var watchlistItem = await _watchlistItemRepository.GetByNormalizedSymbolAsync(normalizedSymbol);

        if (watchlistItem is null || !watchlistItem.IsActive)
        {
            _logger.LogWarning(
                "Active watchlist item was not found for symbol {Symbol}.",
                normalizedSymbol);

            return QuoteRefreshResult.NotFound(normalizedSymbol);
        }

        var quote = await _quoteCacheService.GetQuoteAsync(
            normalizedSymbol,
            cancellationToken);

        var isFromCache = quote is not null;

        if (quote is null)
        {
            quote = await _quoteProvider.GetQuoteAsync(
                normalizedSymbol,
                cancellationToken);

            if (quote is null)
            {
                _logger.LogWarning(
                    "Quote data could not be retrieved for symbol {Symbol}.",
                    normalizedSymbol);

                return QuoteRefreshResult.ExternalFailure(normalizedSymbol);
            }

            await _quoteCacheService.SetQuoteAsync(
                normalizedSymbol,
                quote,
                cancellationToken);
        }

        var priceSnapshot = CreatePriceSnapshot(
            watchlistItem,
            quote);

        await _priceSnapshotRepository.AddAsync(priceSnapshot);
        await _priceSnapshotRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Price snapshot saved for symbol {Symbol}.",
            normalizedSymbol);

        var response = CreateQuoteRefreshResponse(
            quote,
            priceSnapshot,
            isFromCache);

        return QuoteRefreshResult.Success(response);
    }

    private static PriceSnapshot CreatePriceSnapshot(
        WatchlistItem watchlistItem,
        QuoteResponse quote)
    {
        return new PriceSnapshot
        {
            WatchlistItemId = watchlistItem.Id,
            Symbol = watchlistItem.NormalizedSymbol,
            Price = quote.CurrentPrice,
            Currency = DefaultCurrency,
            Source = quote.Source,
            RetrievedAtUtc = quote.RetrievedAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static QuoteRefreshResponse CreateQuoteRefreshResponse(
        QuoteResponse quote,
        PriceSnapshot priceSnapshot,
        bool isFromCache)
    {
        return new QuoteRefreshResponse
        {
            Symbol = quote.Symbol,
            CurrentPrice = quote.CurrentPrice,
            Currency = priceSnapshot.Currency,
            Change = quote.Change,
            PercentChange = quote.PercentChange,
            HighPriceOfDay = quote.HighPriceOfDay,
            LowPriceOfDay = quote.LowPriceOfDay,
            OpenPriceOfDay = quote.OpenPriceOfDay,
            PreviousClosePrice = quote.PreviousClosePrice,
            IsFromCache = isFromCache,
            Source = quote.Source,
            RetrievedAtUtc = quote.RetrievedAtUtc,
            PriceSnapshotId = priceSnapshot.Id,
            SnapshotCreatedAtUtc = priceSnapshot.CreatedAtUtc
        };
    }

    private static string NormalizeSymbol(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }
}