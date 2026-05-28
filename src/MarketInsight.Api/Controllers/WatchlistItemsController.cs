using MarketInsight.Api.DTOs.Watchlist;
using MarketInsight.Api.Services;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchlistItemsController"/> class.
    /// </summary>
    /// <param name="watchlistItemService">The watchlist item service.</param>
    public WatchlistItemsController(IWatchlistItemService watchlistItemService)
    {
        _watchlistItemService = watchlistItemService;
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
    /// Creates a new watchlist item.
    /// </summary>
    /// <param name="request">The watchlist item creation request.</param>
    /// <returns>The created watchlist item.</returns>
    [HttpPost]
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

        return Created(
            $"/api/watchlist-items/{result.Item!.Symbol}",
            result.Item);
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