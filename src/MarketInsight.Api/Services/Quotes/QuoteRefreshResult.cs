using MarketInsight.Api.DTOs.Quotes;

namespace MarketInsight.Api.Services.Quotes;

/// <summary>
/// Represents the application-level result of a quote refresh operation.
/// </summary>
public class QuoteRefreshResult
{
    private QuoteRefreshResult(
        bool isSuccess,
        bool isNotFound,
        bool isExternalFailure,
        QuoteRefreshResponse? response,
        string message)
    {
        IsSuccess = isSuccess;
        IsNotFound = isNotFound;
        IsExternalFailure = isExternalFailure;
        Response = response;
        Message = message;
    }

    /// <summary>
    /// Indicates whether the quote refresh operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the requested symbol was missing or inactive.
    /// </summary>
    public bool IsNotFound { get; }

    /// <summary>
    /// Indicates whether quote data could not be retrieved from the external provider.
    /// </summary>
    public bool IsExternalFailure { get; }

    /// <summary>
    /// Successful quote refresh response.
    /// </summary>
    public QuoteRefreshResponse? Response { get; }

    /// <summary>
    /// Application-level result message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a successful quote refresh result.
    /// </summary>
    /// <param name="response">The successful quote refresh response.</param>
    /// <returns>A successful quote refresh result.</returns>
    public static QuoteRefreshResult Success(QuoteRefreshResponse response)
    {
        return new QuoteRefreshResult(
            isSuccess: true,
            isNotFound: false,
            isExternalFailure: false,
            response: response,
            message: "Quote refresh completed successfully.");
    }

    /// <summary>
    /// Creates a not found result for missing or inactive symbols.
    /// </summary>
    /// <param name="symbol">The requested symbol.</param>
    /// <returns>A not found quote refresh result.</returns>
    public static QuoteRefreshResult NotFound(string symbol)
    {
        return new QuoteRefreshResult(
            isSuccess: false,
            isNotFound: true,
            isExternalFailure: false,
            response: null,
            message: $"Active watchlist item was not found for symbol {symbol}.");
    }

    /// <summary>
    /// Creates a failure result when quote data cannot be retrieved.
    /// </summary>
    /// <param name="symbol">The requested symbol.</param>
    /// <returns>An external failure quote refresh result.</returns>
    public static QuoteRefreshResult ExternalFailure(string symbol)
    {
        return new QuoteRefreshResult(
            isSuccess: false,
            isNotFound: false,
            isExternalFailure: true,
            response: null,
            message: $"Quote data could not be retrieved for symbol {symbol}.");
    }
}