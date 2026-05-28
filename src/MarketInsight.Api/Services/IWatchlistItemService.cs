using MarketInsight.Api.DTOs.Watchlist;

namespace MarketInsight.Api.Services;

/// <summary>
/// Defines business operations for watchlist items.
/// </summary>
public interface IWatchlistItemService
{
    /// <summary>
    /// Gets all active watchlist items.
    /// </summary>
    /// <returns>A list of active watchlist item responses.</returns>
    Task<List<WatchlistItemResponse>> GetAllAsync();

    /// <summary>
    /// Gets a watchlist item by symbol.
    /// </summary>
    /// <param name="symbol">The requested symbol value.</param>
    /// <returns>The matching watchlist item response if found; otherwise null.</returns>
    Task<WatchlistItemResponse?> GetBySymbolAsync(string symbol);

    /// <summary>
    /// Creates a new watchlist item.
    /// </summary>
    /// <param name="request">The watchlist item creation request.</param>
    /// <returns>The creation operation result.</returns>
    Task<CreateWatchlistItemResult> CreateAsync(CreateWatchlistItemRequest request);

    /// <summary>
    /// Deletes a watchlist item by symbol.
    /// </summary>
    /// <param name="symbol">The requested symbol value.</param>
    /// <returns>True if the item was deleted; otherwise false.</returns>
    Task<bool> DeleteAsync(string symbol);
}