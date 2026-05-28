using MarketInsight.Api.DTOs.Watchlist;
using MarketInsight.Api.Entities;
using MarketInsight.Api.Repositories;

namespace MarketInsight.Api.Services;

/// <summary>
/// Provides business operations for watchlist items.
/// </summary>
public class WatchlistItemService : IWatchlistItemService
{
    private readonly IWatchlistItemRepository _watchlistItemRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchlistItemService"/> class.
    /// </summary>
    /// <param name="watchlistItemRepository">The watchlist item repository.</param>
    public WatchlistItemService(IWatchlistItemRepository watchlistItemRepository)
    {
        _watchlistItemRepository = watchlistItemRepository;
    }

    /// <inheritdoc />
    public async Task<List<WatchlistItemResponse>> GetAllAsync()
    {
        var items = await _watchlistItemRepository.GetAllActiveAsync();

        return items
            .Select(MapToResponse)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<WatchlistItemResponse?> GetBySymbolAsync(string symbol)
    {
        var normalizedSymbol = NormalizeSymbol(symbol);

        var item = await _watchlistItemRepository.GetByNormalizedSymbolAsync(normalizedSymbol);

        if (item is null || !item.IsActive)
        {
            return null;
        }

        return MapToResponse(item);
    }

    /// <inheritdoc />
    public async Task<CreateWatchlistItemResult> CreateAsync(CreateWatchlistItemRequest request)
    {
        var normalizedSymbol = NormalizeSymbol(request.Symbol);

        var exists = await _watchlistItemRepository.ExistsByNormalizedSymbolAsync(normalizedSymbol);

        if (exists)
        {
            return CreateWatchlistItemResult.Duplicate(normalizedSymbol);
        }

        var watchlistItem = new WatchlistItem
        {
            Symbol = normalizedSymbol,
            NormalizedSymbol = normalizedSymbol,
            DisplayName = NormalizeOptionalText(request.DisplayName),
            Market = NormalizeOptionalText(request.Market),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _watchlistItemRepository.AddAsync(watchlistItem);
        await _watchlistItemRepository.SaveChangesAsync();

        return CreateWatchlistItemResult.Created(MapToResponse(watchlistItem));
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string symbol)
    {
        var normalizedSymbol = NormalizeSymbol(symbol);

        var item = await _watchlistItemRepository.GetByNormalizedSymbolAsync(normalizedSymbol);

        if (item is null || !item.IsActive)
        {
            return false;
        }

        item.IsActive = false;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _watchlistItemRepository.SaveChangesAsync();

        return true;
    }

    private static string NormalizeSymbol(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static WatchlistItemResponse MapToResponse(WatchlistItem item)
    {
        return new WatchlistItemResponse
        {
            Id = item.Id,
            Symbol = item.Symbol,
            DisplayName = item.DisplayName,
            Market = item.Market,
            IsActive = item.IsActive,
            CreatedAtUtc = item.CreatedAtUtc,
            UpdatedAtUtc = item.UpdatedAtUtc
        };
    }
}