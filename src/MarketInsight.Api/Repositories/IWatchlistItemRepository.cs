using MarketInsight.Api.Entities;

namespace MarketInsight.Api.Repositories;

/// <summary>
/// Defines database access operations for watchlist items.
/// </summary>
public interface IWatchlistItemRepository
{
    /// <summary>
    /// Gets all active watchlist items from the database.
    /// </summary>
    /// <returns>A list of active watchlist items.</returns>
    Task<List<WatchlistItem>> GetAllActiveAsync();

    /// <summary>
    /// Gets a watchlist item by its normalized symbol.
    /// </summary>
    /// <param name="normalizedSymbol">The normalized symbol value.</param>
    /// <returns>The matching watchlist item if found; otherwise null.</returns>
    Task<WatchlistItem?> GetByNormalizedSymbolAsync(string normalizedSymbol);

    /// <summary>
    /// Checks whether a watchlist item exists for the given normalized symbol.
    /// </summary>
    /// <param name="normalizedSymbol">The normalized symbol value.</param>
    /// <returns>True if the symbol exists; otherwise false.</returns>
    Task<bool> ExistsByNormalizedSymbolAsync(string normalizedSymbol);

    /// <summary>
    /// Adds a new watchlist item to the database context.
    /// </summary>
    /// <param name="watchlistItem">The watchlist item entity to add.</param>
    Task AddAsync(WatchlistItem watchlistItem);

    /// <summary>
    /// Saves pending database changes.
    /// </summary>
    Task SaveChangesAsync();
}