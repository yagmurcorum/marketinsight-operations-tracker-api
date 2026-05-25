namespace MarketInsight.Api.DTOs.Watchlist;

/// <summary>
/// Represents the response model returned for a watchlist item.
/// </summary>
public class WatchlistItemResponse
{
    /// <summary>
    /// Unique identifier of the watchlist item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Financial symbol stored in the watchlist.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Normalized financial symbol used by the system.
    /// </summary>
    public string NormalizedSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name for the financial symbol.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional market information for the financial symbol.
    /// </summary>
    public string? Market { get; set; }

    /// <summary>
    /// Indicates whether the symbol is active in the watchlist.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// UTC date and time when the watchlist item was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// UTC date and time when the watchlist item was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}