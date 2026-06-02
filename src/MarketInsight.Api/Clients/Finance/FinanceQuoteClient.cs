using System.Net.Http.Json;
using System.Text.Json;
using MarketInsight.Api.DTOs.Quotes;
using Microsoft.Extensions.Logging;

namespace MarketInsight.Api.Clients.Finance;

/// <summary>
/// Retrieves quote data from the configured external finance API.
/// </summary>
public class FinanceQuoteClient : IFinanceQuoteClient
{
    private const string SourceName = "Finnhub";

    private readonly HttpClient _httpClient;
    private readonly ILogger<FinanceQuoteClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceQuoteClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to call the external finance API.</param>
    /// <param name="logger">The logger instance.</param>
    public FinanceQuoteClient(
        HttpClient httpClient,
        ILogger<FinanceQuoteClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<QuoteResponse?> GetQuoteAsync(
        string symbol,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Finance API key is missing.");
            return null;
        }

        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        var requestUri =
            $"quote?symbol={Uri.EscapeDataString(normalizedSymbol)}&token={Uri.EscapeDataString(apiKey)}";

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Finance API request failed for symbol {Symbol} with status code {StatusCode}.",
                    normalizedSymbol,
                    response.StatusCode);

                return null;
            }

            var externalQuote = await response.Content.ReadFromJsonAsync<FinnhubQuoteResponse>(
                cancellationToken: cancellationToken);

            if (externalQuote is null || externalQuote.CurrentPrice <= 0)
            {
                _logger.LogWarning(
                    "Finance API returned empty or invalid quote data for symbol {Symbol}.",
                    normalizedSymbol);

                return null;
            }

            return MapToQuoteResponse(normalizedSymbol, externalQuote);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Finance API request timed out for symbol {Symbol}.",
                normalizedSymbol);

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Finance API HTTP request failed for symbol {Symbol}.",
                normalizedSymbol);

            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Finance API returned invalid JSON for symbol {Symbol}.",
                normalizedSymbol);

            return null;
        }
    }

    private static QuoteResponse MapToQuoteResponse(
        string symbol,
        FinnhubQuoteResponse externalQuote)
    {
        return new QuoteResponse
        {
            Symbol = symbol,
            CurrentPrice = externalQuote.CurrentPrice,
            Change = externalQuote.Change,
            PercentChange = externalQuote.PercentChange,
            HighPriceOfDay = externalQuote.HighPriceOfDay,
            LowPriceOfDay = externalQuote.LowPriceOfDay,
            OpenPriceOfDay = externalQuote.OpenPriceOfDay,
            PreviousClosePrice = externalQuote.PreviousClosePrice,
            RetrievedAtUtc = ResolveRetrievedAtUtc(externalQuote.UnixTimestamp),
            Source = SourceName
        };
    }

    private static DateTime ResolveRetrievedAtUtc(long unixTimestamp)
    {
        if (unixTimestamp <= 0)
        {
            return DateTime.UtcNow;
        }

        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
    }
}