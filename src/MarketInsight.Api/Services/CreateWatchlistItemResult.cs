using MarketInsight.Api.DTOs.Watchlist;

namespace MarketInsight.Api.Services;

/// <summary>
/// Represents the result of a watchlist item creation operation.
/// </summary>
public class CreateWatchlistItemResult
{
    /// <summary>
    /// Gets a value indicating whether the watchlist item was created successfully.
    /// </summary>
    public bool IsCreated { get; init; }

    /// <summary>
    /// Gets a value indicating whether the requested symbol already exists.
    /// </summary>
    public bool IsDuplicate { get; init; }

    /// <summary>
    /// Gets the created watchlist item response when the operation succeeds.
    /// </summary>
    public WatchlistItemResponse? Item { get; init; }

    /// <summary>
    /// Gets the operation message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Creates a successful creation result.
    /// </summary>
    /// <param name="item">The created watchlist item response.</param>
    /// <returns>A successful create result.</returns>
    public static CreateWatchlistItemResult Created(WatchlistItemResponse item)
    {
        return new CreateWatchlistItemResult
        {
            IsCreated = true,
            IsDuplicate = false,
            Item = item,
            Message = "Watchlist item was created successfully."
        };
    }

    /// <summary>
    /// Creates a duplicate symbol result.
    /// </summary>
    /// <param name="normalizedSymbol">The normalized symbol that already exists.</param>
    /// <returns>A duplicate create result.</returns>
    public static CreateWatchlistItemResult Duplicate(string normalizedSymbol)
    {
        return new CreateWatchlistItemResult
        {
            IsCreated = false,
            IsDuplicate = true,
            Item = null,
            Message = $"Watchlist item with symbol '{normalizedSymbol}' already exists."
        };
    }
}