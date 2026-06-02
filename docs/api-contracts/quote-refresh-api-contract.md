# Quote Refresh API Contract

This document defines the quote refresh contract for active watchlist symbols in the MarketInsight Operations Tracker API.

The current implementation covers the application service layer, Redis cache-aside flow, and PriceSnapshot persistence. Controller endpoint implementation will be handled separately.

---

## Purpose

The quote refresh use case retrieves the latest quote data for an active `WatchlistItem`.

A successful refresh operation:

- validates that the symbol belongs to an active watchlist item
- checks Redis cache before calling the external finance provider
- uses the quote provider only on cache miss
- stores successful quote data in Redis
- saves a `PriceSnapshot` record in SQLite
- returns a controlled response DTO

SQLite remains the persistent storage. Redis is used only for short-term quote caching.

---

## Planned Endpoint

The controller endpoint is out of scope for the current issue.

Planned route for the next implementation step:

```http
POST /api/watchlist-items/{symbol}/refresh
```

Example:

```http
POST /api/watchlist-items/AAPL/refresh
```

---

## Request

The symbol is provided through the route.

| Field | Source | Required | Description |
|---|---|---:|---|
| symbol | Route | Yes | Financial symbol to refresh |

Example symbol normalization:

```text
aapl → AAPL
```

---

## Successful Response

HTTP status planned for successful refresh:

```http
200 OK
```

Response DTO:

```csharp
QuoteRefreshResponse
```

Example response:

```json
{
  "symbol": "AAPL",
  "currentPrice": 212.45,
  "currency": "USD",
  "change": 1.25,
  "percentChange": 0.59,
  "highPriceOfDay": 214.1,
  "lowPriceOfDay": 209.8,
  "openPriceOfDay": 210.3,
  "previousClosePrice": 211.2,
  "isFromCache": false,
  "source": "Finnhub",
  "retrievedAtUtc": "2026-06-02T16:10:00Z",
  "priceSnapshotId": 15,
  "snapshotCreatedAtUtc": "2026-06-02T16:10:01Z"
}
```

---

## Response Fields

| Field | Description |
|---|---|
| symbol | Normalized financial symbol |
| currentPrice | Current quote price |
| currency | Quote currency |
| change | Price change |
| percentChange | Price change percentage |
| highPriceOfDay | Highest price of the day |
| lowPriceOfDay | Lowest price of the day |
| openPriceOfDay | Opening price of the day |
| previousClosePrice | Previous close price |
| isFromCache | Indicates whether Redis returned the quote |
| source | External quote provider |
| retrievedAtUtc | Time returned by the quote provider |
| priceSnapshotId | Saved SQLite snapshot identifier |
| snapshotCreatedAtUtc | Time when the snapshot was saved |

---

## Controlled Failure Responses

The service returns controlled application-level results. HTTP mapping will be done in the controller issue.

### Missing or Inactive Symbol

If the symbol does not exist or the watchlist item is inactive, the external provider must not be called.

Planned HTTP status:

```http
404 Not Found
```

Example message:

```json
{
  "message": "Active watchlist item was not found for symbol AAPL."
}
```

### External Quote Failure

If the active watchlist item exists but quote data cannot be retrieved, the operation fails safely.

Planned HTTP status:

```http
502 Bad Gateway
```

Example message:

```json
{
  "message": "Quote data could not be retrieved for symbol AAPL."
}
```

---

## Refresh Flow

```text
Normalize symbol
      ↓
Check active WatchlistItem
      ↓
If missing or inactive:
      return controlled not found result
      ↓
Check Redis cache with quote:{symbol}
      ↓
If cache hit:
      use cached quote
      ↓
If cache miss:
      call quote provider
      ↓
If quote provider succeeds:
      write quote to Redis
      ↓
Save PriceSnapshot to SQLite
      ↓
Return QuoteRefreshResponse
```

---

## Redis Cache Behavior

Redis uses cache-aside pattern.

| Standard | Value |
|---|---|
| Key format | `quote:{symbol}` |
| Example key | `quote:AAPL` |
| TTL | 5 minutes |
| Role | Short-term quote cache |
| Persistent storage | No |

Redis is checked before the external finance provider is called.

A cache hit avoids an external API call.

A cache miss calls the configured quote provider.

---

## PriceSnapshot Persistence

Every successful refresh saves a `PriceSnapshot` record to SQLite.

This applies to both:

- cache hit
- cache miss with successful external quote response

The saved snapshot must be linked to the active `WatchlistItem`.

Expected persisted fields:

| Field | Source |
|---|---|
| WatchlistItemId | Active WatchlistItem |
| Symbol | Normalized symbol |
| Price | QuoteResponse.CurrentPrice |
| Currency | Default quote currency |
| Source | Quote provider |
| RetrievedAtUtc | Quote response |
| CreatedAtUtc | Application timestamp |

---

## Service Layer Contract

Implemented service contract:

```csharp
IQuoteRefreshService
```

Primary method:

```csharp
Task<QuoteRefreshResult> RefreshQuoteAsync(
    string symbol,
    CancellationToken cancellationToken = default);
```

The service is responsible for:

- symbol normalization
- active watchlist validation
- Redis cache lookup
- quote provider call on cache miss
- Redis cache write after successful provider response
- PriceSnapshot persistence
- controlled application result creation

---

## Repository Contract

Implemented repository contract:

```csharp
IPriceSnapshotRepository
```

Responsibilities:

- add new `PriceSnapshot` records
- save database changes through EF Core

The repository does not contain quote refresh business logic.

---

## Logging Standard

The refresh flow should produce structured logs for key operations.

Expected log events:

```text
Price refresh requested for symbol AAPL.
Cache hit for symbol AAPL.
Cache miss for symbol AAPL.
Quote cached for symbol AAPL.
Price snapshot saved for symbol AAPL.
```

Missing or inactive symbols should also be logged without calling the external provider.

---

## Out of Scope

This contract does not include:

- controller implementation
- snapshot listing endpoint
- RabbitMQ
- Background Worker
- alert evaluation
- cache management endpoint
- PUT/PATCH WatchlistItem update endpoint

---

## Review Checklist

Before closing the issue, verify:

- `IQuoteRefreshService` exists.
- `QuoteRefreshService` exists.
- `IPriceSnapshotRepository` exists.
- `PriceSnapshotRepository` exists.
- Only active WatchlistItem records can be refreshed.
- Missing symbols do not call the external provider.
- Inactive symbols do not call the external provider.
- Redis is checked before the external provider.
- Cache miss calls the quote provider.
- Successful quote data is written to Redis.
- Successful refresh saves a PriceSnapshot.
- PriceSnapshot is linked to the active WatchlistItem.
- Structured logs exist for refresh, cache hit, cache miss, and snapshot save.
- Project builds successfully.

---

## Summary

The quote refresh service prepares the core business flow for refreshing active watchlist symbols.

The implementation keeps responsibilities separated:

```text
QuoteRefreshService = business flow
IQuoteCacheService = Redis cache access
IQuoteProvider = external quote retrieval
IPriceSnapshotRepository = SQLite snapshot persistence
```

The next step is to expose this use case through a controller endpoint.