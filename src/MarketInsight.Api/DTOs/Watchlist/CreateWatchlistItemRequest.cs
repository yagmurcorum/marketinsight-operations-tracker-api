using System.ComponentModel.DataAnnotations;

namespace MarketInsight.Api.DTOs.Watchlist;

/// <summary>
/// Represents the request body used to add a new financial symbol to the watchlist.
/// </summary>
public class CreateWatchlistItemRequest 
{
    /// <summary>
    /// Financial symbol requested by the user, such as AAPL or TSLA.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name for the financial symbol.
    /// </summary>
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional market information for the financial symbol.
    /// </summary>
    [MaxLength(50)]
    public string? Market { get; set; }
}