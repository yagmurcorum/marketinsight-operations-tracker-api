using MarketInsight.Api.Entities;

namespace MarketInsight.Api.Repositories;

/// <summary>
/// Defines database access operations for price snapshots.
/// </summary>
public interface IPriceSnapshotRepository
{
    /// <summary>
    /// Gets saved price snapshots for the requested watchlist item.
    /// </summary>
    /// <param name="watchlistItemId">The related watchlist item identifier.</param>
    /// <returns>A list of saved price snapshots.</returns>
    Task<List<PriceSnapshot>> GetByWatchlistItemIdAsync(int watchlistItemId);

    /// <summary>
    /// Adds a new price snapshot to the database context.
    /// </summary>
    /// <param name="priceSnapshot">The price snapshot entity to add.</param>
    Task AddAsync(PriceSnapshot priceSnapshot);

    /// <summary>
    /// Saves pending database changes.
    /// </summary>
    Task SaveChangesAsync();
}