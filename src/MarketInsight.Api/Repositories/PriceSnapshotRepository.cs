using MarketInsight.Api.Data;
using MarketInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketInsight.Api.Repositories;

/// <summary>
/// Provides EF Core based data access operations for price snapshots.
/// </summary>
public class PriceSnapshotRepository : IPriceSnapshotRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceSnapshotRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public PriceSnapshotRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<PriceSnapshot>> GetByWatchlistItemIdAsync(int watchlistItemId)
    {
        return await _context.PriceSnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.WatchlistItemId == watchlistItemId)
            .OrderByDescending(snapshot => snapshot.CreatedAtUtc)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(PriceSnapshot priceSnapshot)
    {
        await _context.PriceSnapshots.AddAsync(priceSnapshot);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}