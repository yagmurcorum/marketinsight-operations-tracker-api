using System.Text.Json;
using MarketInsight.Api.DTOs.Quotes;
using StackExchange.Redis;

namespace MarketInsight.Api.Services.Cache;

/// <summary>
/// Stores and retrieves quote data from Redis cache.
/// </summary>
public class RedisQuoteCacheService : IQuoteCacheService
{
    private static readonly TimeSpan QuoteCacheTtl = TimeSpan.FromMinutes(5);

    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisQuoteCacheService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisQuoteCacheService"/> class.
    /// </summary>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    /// <param name="logger">The logger instance.</param>
    public RedisQuoteCacheService(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisQuoteCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<QuoteResponse?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(symbol))
        {
            return null;
        }

        var cacheKey = BuildCacheKey(symbol);
        var database = _connectionMultiplexer.GetDatabase();

        var cachedValue = await database.StringGetAsync(cacheKey);

        if (cachedValue.IsNullOrEmpty)
        {
            _logger.LogInformation(
                "Cache miss for symbol {Symbol}.",
                NormalizeSymbol(symbol));

            return null;
        }

        try
        {
            var quote = JsonSerializer.Deserialize<QuoteResponse>(cachedValue!);

            _logger.LogInformation(
                "Cache hit for symbol {Symbol}.",
                NormalizeSymbol(symbol));

            return quote;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Cached quote data could not be deserialized for symbol {Symbol}.",
                NormalizeSymbol(symbol));

            await database.KeyDeleteAsync(cacheKey);

            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetQuoteAsync(
        string symbol,
        QuoteResponse quote,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(symbol))
        {
            return;
        }

        var cacheKey = BuildCacheKey(symbol);
        var database = _connectionMultiplexer.GetDatabase();

        var serializedQuote = JsonSerializer.Serialize(quote);

        await database.StringSetAsync(
            cacheKey,
            serializedQuote,
            QuoteCacheTtl);

        _logger.LogInformation(
            "Quote cached for symbol {Symbol}.",
            NormalizeSymbol(symbol));
    }

    private static string BuildCacheKey(string symbol)
    {
        return $"quote:{NormalizeSymbol(symbol)}";
    }

    private static string NormalizeSymbol(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }
}