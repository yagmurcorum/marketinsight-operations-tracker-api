namespace MarketInsight.Api.Entities;

/// <summary>
/// Represents a financial symbol tracked by the user.
/// </summary>
public class WatchlistItem
{
    /// <summary>
    /// Unique identifier of the watchlist item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Original financial symbol value received by the API.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Normalized financial symbol used for lookup, comparison, and duplicate checks.
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
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC date and time when the watchlist item was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC date and time when the watchlist item was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}