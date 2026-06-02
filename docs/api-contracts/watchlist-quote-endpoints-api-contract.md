# Watchlist Quote Endpoints API Contract

This document defines the API contract for refreshing quote data and listing saved price snapshots for active watchlist symbols.

The endpoints follow the existing watchlist route standard and are testable through Swagger.

---

## Purpose

The purpose of these endpoints is to expose quote refresh and snapshot listing operations through `WatchlistItemsController`.

The endpoints support:

- refreshing quote data for an active watchlist symbol
- saving successful refresh results as price snapshots
- listing saved price snapshots for an active watchlist symbol
- returning DTO responses instead of Entity models
- returning controlled `404 Not Found` responses for missing or inactive symbols

---

## Route Standard

Base route:

```text
/api/watchlist-items
```

Implemented endpoints:

```http
POST /api/watchlist-items/{symbol}/refresh
GET /api/watchlist-items/{symbol}/snapshots
```

The following route styles are intentionally not used:

```http
POST /api/quotes/{symbol}/refresh
GET /api/price-snapshots/{symbol}
```

---

## Endpoint: Refresh Quote

```http
POST /api/watchlist-items/{symbol}/refresh
```

Refreshes quote data for an active watchlist symbol.

### Request

| Field | Source | Required | Description |
|---|---|---:|---|
| symbol | Route | Yes | Financial symbol to refresh |

Example:

```http
POST /api/watchlist-items/AAPL/refresh
```

### Success Response

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

### Not Found Response

Returned when the symbol does not exist or the related watchlist item is inactive.

```http
404 Not Found
```

Example response:

```json
{
  "message": "Active watchlist item was not found for symbol AAPL."
}
```

### External Provider Failure

Returned when the symbol is active but quote data cannot be retrieved from the configured quote provider.

```http
502 Bad Gateway
```

Example response:

```json
{
  "message": "Quote data could not be retrieved for symbol AAPL."
}
```

---

## Endpoint: List Price Snapshots

```http
GET /api/watchlist-items/{symbol}/snapshots
```

Returns saved price snapshots for an active watchlist symbol.

### Request

| Field | Source | Required | Description |
|---|---|---:|---|
| symbol | Route | Yes | Financial symbol whose snapshots will be listed |

Example:

```http
GET /api/watchlist-items/AAPL/snapshots
```

### Success Response

```http
200 OK
```

Response DTO:

```csharp
List<PriceSnapshotResponse>
```

Example response:

```json
[
  {
    "id": 15,
    "watchlistItemId": 3,
    "symbol": "AAPL",
    "price": 212.45,
    "currency": "USD",
    "source": "Finnhub",
    "retrievedAtUtc": "2026-06-02T16:10:00Z",
    "createdAtUtc": "2026-06-02T16:10:01Z"
  }
]
```

### Empty Snapshot List

If the symbol is active but has no saved snapshots, the endpoint returns:

```http
200 OK
```

Body:

```json
[]
```

### Not Found Response

Returned when the symbol does not exist or the related watchlist item is inactive.

```http
404 Not Found
```

Example response:

```json
{
  "message": "Active watchlist item was not found for symbol AAPL."
}
```

---

## Response DTOs

Controller responses use DTOs only.

| Endpoint | Response DTO |
|---|---|
| `POST /api/watchlist-items/{symbol}/refresh` | `QuoteRefreshResponse` |
| `GET /api/watchlist-items/{symbol}/snapshots` | `List<PriceSnapshotResponse>` |

Entity models are not returned directly from these endpoints.

---

## Controller Responsibility

`WatchlistItemsController` is responsible for the HTTP contract only.

Responsibilities:

- expose refresh and snapshot listing endpoints
- call `IQuoteRefreshService`
- map service results to HTTP responses
- return DTO responses
- provide XML Summary and Swagger response metadata

The controller does not contain Redis, external API, or persistence logic.

---

## Refresh Flow

```text
POST /api/watchlist-items/{symbol}/refresh
      ↓
WatchlistItemsController
      ↓
IQuoteRefreshService.RefreshQuoteAsync
      ↓
Active WatchlistItem validation
      ↓
Redis cache lookup
      ↓
Quote provider call on cache miss
      ↓
PriceSnapshot persistence
      ↓
QuoteRefreshResponse
```

---

## Snapshot Listing Flow

```text
GET /api/watchlist-items/{symbol}/snapshots
      ↓
WatchlistItemsController
      ↓
IQuoteRefreshService.GetSnapshotsAsync
      ↓
Active WatchlistItem validation
      ↓
PriceSnapshotRepository
      ↓
List<PriceSnapshotResponse>
```

---

## Swagger Metadata

The endpoints include XML Summary documentation and response metadata.

Expected Swagger endpoints:

```http
POST /api/watchlist-items/{symbol}/refresh
GET /api/watchlist-items/{symbol}/snapshots
```

Expected status codes:

| Endpoint | Status Codes |
|---|---|
| POST refresh | `200`, `404`, `502` |
| GET snapshots | `200`, `404` |

---

## Swagger Test Results

The endpoints were verified through Swagger.

| Test Case | Expected Result | Status |
|---|---|---|
| Refresh active symbol | `200 OK` | Passed |
| Refresh missing symbol | `404 Not Found` | Passed |
| Refresh inactive symbol | `404 Not Found` | Passed |
| Refresh returns `QuoteRefreshResponse` | DTO response | Passed |
| Refresh creates `PriceSnapshot` | Snapshot saved | Passed |
| List snapshots for active symbol | `200 OK` | Passed |
| List snapshots for missing symbol | `404 Not Found` | Passed |
| List snapshots for inactive symbol | `404 Not Found` | Passed |
| Empty snapshot list for active symbol | `200 OK` with `[]` | Passed |
| Snapshot listing returns DTO list | `List<PriceSnapshotResponse>` | Passed |
| Entity models are not returned directly | DTO only | Passed |
| XML Summary appears in Swagger | Visible | Passed |

---

## Out of Scope

This implementation does not include:

- `POST /api/quotes/{symbol}/refresh`
- `GET /api/price-snapshots/{symbol}`
- RabbitMQ async refresh endpoint
- Background Worker
- alert evaluation
- PUT/PATCH WatchlistItem update endpoint

---

## Summary

The watchlist quote endpoints expose quote refresh and price snapshot listing through the existing `/api/watchlist-items` route standard.

Implemented endpoints:

```http
POST /api/watchlist-items/{symbol}/refresh
GET /api/watchlist-items/{symbol}/snapshots
```

The implementation keeps responsibilities separated:

```text
Controller = HTTP contract
QuoteRefreshService = business flow
RedisQuoteCacheService = cache access
PriceSnapshotRepository = snapshot persistence
DTOs = API response shape
```