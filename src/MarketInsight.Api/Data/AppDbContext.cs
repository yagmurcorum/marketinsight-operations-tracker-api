using MarketInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketInsight.Api.Data;

/// <summary>
/// Represents the Entity Framework Core database context for the MarketInsight API.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AppDbContext class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Tracked financial symbols stored in the watchlist.
    /// </summary>
    public DbSet<WatchlistItem> WatchlistItems { get; set; } = null!;

    /// <summary>
    /// Historical price snapshots for tracked financial symbols.
    /// </summary>
    public DbSet<PriceSnapshot> PriceSnapshots { get; set; } = null!;

    /// <summary>
    /// Price alert rules defined for tracked financial symbols.
    /// </summary>
    public DbSet<PriceAlert> PriceAlerts { get; set; } = null!;

    /// <summary>
    /// Follow-up actions created after price alerts or important price events.
    /// </summary>
    public DbSet<ActionItem> ActionItems { get; set; } = null!;
}