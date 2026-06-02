using MarketInsight.Api.Entities;

namespace MarketInsight.Api.Repositories;

/// <summary>
/// Defines database access operations for price snapshots.
/// </summary>
public interface IPriceSnapshotRepository
{
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