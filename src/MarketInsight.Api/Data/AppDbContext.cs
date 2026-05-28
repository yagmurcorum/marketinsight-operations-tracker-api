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

    /// <summary>
    /// Configures entity constraints, indexes, and relationships.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WatchlistItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.NormalizedSymbol)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(x => x.NormalizedSymbol)
                .IsUnique();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(100);

            entity.Property(x => x.Market)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<PriceSnapshot>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Price)
                .IsRequired();

            entity.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.Source)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(x => x.WatchlistItem)
                .WithMany()
                .HasForeignKey(x => x.WatchlistItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceAlert>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.ConditionType)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.TargetPrice)
                .IsRequired();

            entity.HasOne(x => x.WatchlistItem)
                .WithMany()
                .HasForeignKey(x => x.WatchlistItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActionItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasOne(x => x.WatchlistItem)
                .WithMany()
                .HasForeignKey(x => x.WatchlistItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.PriceAlert)
                .WithMany()
                .HasForeignKey(x => x.PriceAlertId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}