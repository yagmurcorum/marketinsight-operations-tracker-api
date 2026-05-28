using MarketInsight.Api.Data;
using MarketInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketInsight.Api.Repositories;

/// <summary>
/// Provides EF Core based data access operations for watchlist items.
/// </summary>
public class WatchlistItemRepository : IWatchlistItemRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchlistItemRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public WatchlistItemRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<WatchlistItem>> GetAllActiveAsync()
    {
        return await _context.WatchlistItems
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.Symbol)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<WatchlistItem?> GetByNormalizedSymbolAsync(string normalizedSymbol)
    {
        return await _context.WatchlistItems
            .FirstOrDefaultAsync(item => item.NormalizedSymbol == normalizedSymbol);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNormalizedSymbolAsync(string normalizedSymbol)
    {
        return await _context.WatchlistItems
            .AnyAsync(item => item.NormalizedSymbol == normalizedSymbol);
    }

    /// <inheritdoc />
    public async Task AddAsync(WatchlistItem watchlistItem)
    {
        await _context.WatchlistItems.AddAsync(watchlistItem);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
