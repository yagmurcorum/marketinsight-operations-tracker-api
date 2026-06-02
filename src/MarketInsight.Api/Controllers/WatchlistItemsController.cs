using MarketInsight.Api.DTOs.Quotes;
using MarketInsight.Api.DTOs.Watchlist;
using MarketInsight.Api.Services;
using MarketInsight.Api.Services.Quotes;
using Microsoft.AspNetCore.Mvc;

namespace MarketInsight.Api.Controllers;

/// <summary>
/// Provides API endpoints for managing watchlist items.
/// </summary>
[ApiController]
[Route("api/watchlist-items")]
public class WatchlistItemsController : ControllerBase
{
    private readonly IWatchlistItemService _watchlistItemService;
    private readonly IQuoteRefreshService _quoteRefreshService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchlistItemsController"/> class.
    /// </summary>
    /// <param name="watchlistItemService">The watchlist item service.</param>
    /// <param name="quoteRefreshService">The quote refresh service.</param>
    public WatchlistItemsController(
        IWatchlistItemService watchlistItemService,
        IQuoteRefreshService quoteRefreshService)
    {
        _watchlistItemService = watchlistItemService;
        _quoteRefreshService = quoteRefreshService;
    }

    /// <summary>
    /// Gets all active watchlist items.
    /// </summary>
    /// <returns>A list of active watchlist items.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<WatchlistItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WatchlistItemResponse>>> GetWatchlistItemsAsync()
    {
        var items = await _watchlistItemService.GetAllAsync();

        return Ok(items);
    }

    /// <summary>
    /// Gets a watchlist item by symbol.
    /// </summary>
    /// <param name="symbol">The requested symbol value.</param>
    /// <returns>The matching watchlist item if found.</returns>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(WatchlistItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WatchlistItemResponse>> GetWatchlistItemBySymbolAsync(string symbol)
    {
        var item = await _watchlistItemService.GetBySymbolAsync(symbol);

        if (item is null)
        {
            return NotFound(new
            {
                message = $"Watchlist item with symbol '{symbol}' was not found."
            });
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a new watchlist item or reactivates an inactive existing watchlist item.
    /// </summary>
    /// <param name="request">The watchlist item creation request.</param>
    /// <returns>The created or reactivated watchlist item.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(WatchlistItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WatchlistItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WatchlistItemResponse>> CreateWatchlistItemAsync(
        [FromBody] CreateWatchlistItemRequest request)
    {
        var result = await _watchlistItemService.CreateAsync(request);

        if (result.IsDuplicate)
        {
            return Conflict(new
            {
                message = result.Message
            });
        }

        if (result.IsReactivated)
        {
            return Ok(result.Item);
        }

        return Created(
            $"/api/watchlist-items/{result.Item!.Symbol}",
            result.Item);
    }

    /// <summary>
    /// Refreshes quote data for an active watchlist symbol.
    /// </summary>
    /// <param name="symbol">The requested symbol value.</param>
    /// <returns>The refreshed quote response if the symbol is active.</returns>
    [HttpPost("{symbol}/refresh")]
    [ProducesResponseType(typeof(QuoteRefreshResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<QuoteRefreshResponse>> RefreshQuoteAsync(string symbol)
    {
        var result = await _quoteRefreshService.RefreshQuoteAsync(symbol);

        if (result.IsNotFound)
        {
            return NotFound(new
            {
                message = result.Message
            });
        }

        if (result.IsExternalFailure)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message = result.Message
                });
        }

        return Ok(result.Response);
    }

    /// <summary>
    /// Gets saved price snapshots for an active watchlist symbol.
    /// </summary>
    /// <param name="symbol">The requested symbol value.</param>
    /// <returns>A list of saved price snapshots for the active symbol.</returns>
    [HttpGet("{symbol}/snapshots")]
    [ProducesResponseType(typeof(List<PriceSnapshotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PriceSnapshotResponse>>> GetPriceSnapshotsAsync(string symbol)
    {
        var snapshots = await _quoteRefreshService.GetSnapshotsAsync(symbol);

        if (snapshots is null)
        {
            return NotFound(new
            {
                message = $"Active watchlist item was not found for symbol {symbol}."
            });
        }

        return Ok(snapshots);
    }

    /// <summary>
    /// Deletes a watchlist item by symbol.
    /// </summary>
    /// <param name="symbol">The requested symbol value.</param>
    /// <returns>No content if the item was deleted successfully.</returns>
    [HttpDelete("{symbol}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWatchlistItemAsync(string symbol)
    {
        var deleted = await _watchlistItemService.DeleteAsync(symbol);

        if (!deleted)
        {
            return NotFound(new
            {
                message = $"Watchlist item with symbol '{symbol}' was not found."
            });
        }

        return NoContent();
    }
}