# Week 3 External API, Redis Cache and Quote Refresh Summary

This document summarizes the Week 3 implementation for external finance API integration, User Secrets configuration, Redis quote caching, quote refresh flow, PriceSnapshot persistence, and Swagger-based verification.

The goal of this document is to make the Week 3 work reviewable and demo-ready.

---

## Scope

Week 3 focused on the quote refresh infrastructure and API flow.

Completed areas:

- external finance API client foundation
- User Secrets based API key configuration
- simple quote provider abstraction
- Redis cache infrastructure
- cache-aside quote caching
- quote refresh service flow
- PriceSnapshot persistence
- snapshot listing endpoint
- Swagger-based endpoint verification

Out of scope:

- RabbitMQ
- Background Worker
- async refresh endpoint
- alert evaluation
- production Redis hardening
- final demo guide

---

## External Finance API Integration

The project uses an external finance API client to retrieve quote data.

Implemented components:

```text
IFinanceQuoteClient
FinanceQuoteClient
FinnhubQuoteResponse
QuoteResponse
```

The external API response model is separated from the internal API response DTO.

Purpose of the separation:

```text
External API model = provider-specific raw response
Internal DTO = application-controlled response shape
```

This prevents external provider details from leaking directly into the application API contract.

Reference document:

```text
docs/integrations/public-finance-api-integration.md
```

---

## User Secrets Configuration

The finance API key is not stored in source code or committed configuration files.

Configuration split:

| Value | Location |
|---|---|
| Finance API base URL | `appsettings.json` |
| Provider name | `appsettings.json` |
| API key | User Secrets |

Expected local secret key:

```text
FinanceApi:ApiKey
```

This keeps sensitive values outside Git while allowing local development to run normally.

Reference document:

```text
docs/configuration/user-secrets-and-strategy-pattern.md
```

---

## Quote Provider Structure

The quote provider abstraction keeps the refresh flow independent from a specific external API provider.

Implemented components:

```text
IQuoteProvider
FinnhubQuoteProvider
```

Current provider flow:

```text
QuoteRefreshService
      ↓
IQuoteProvider
      ↓
FinnhubQuoteProvider
      ↓
IFinanceQuoteClient
      ↓
FinanceQuoteClient
```

This structure keeps the implementation simple while allowing future provider changes without rewriting the refresh business flow.

---

## Redis Cache Design

Redis is used only as a short-term quote cache.

Redis does not replace SQLite.

| Responsibility | Storage |
|---|---|
| Short-term quote cache | Redis |
| Persistent application data | SQLite |
| Saved quote history | SQLite / PriceSnapshot |

Cache standard:

| Setting | Value |
|---|---|
| Pattern | Cache-aside |
| Key format | `quote:{symbol}` |
| Example key | `quote:AAPL` |
| TTL | 5 minutes |

Reference document:

```text
docs/cache/redis-cache-design.md
```

---

## Cache-Aside Behavior

The quote refresh flow checks Redis before calling the external quote provider.

Expected behavior:

```text
Refresh requested
      ↓
Check Redis cache
      ↓
Cache hit:
      use cached quote
      ↓
Cache miss:
      call quote provider
      ↓
Store successful quote in Redis
      ↓
Save PriceSnapshot
```

A cache hit avoids an external API call.

A cache miss retrieves fresh quote data and writes it back to Redis with a 5-minute TTL.

---

## Quote Refresh Service Flow

The quote refresh business flow is handled by:

```text
IQuoteRefreshService
QuoteRefreshService
```

Main responsibilities:

- normalize symbol
- validate active WatchlistItem
- return controlled result for missing or inactive symbols
- check Redis cache before external provider call
- call quote provider on cache miss
- cache successful quote response
- persist successful refresh as PriceSnapshot
- return QuoteRefreshResponse DTO

Reference document:

```text
docs/api-contracts/quote-refresh-api-contract.md
```

---

## PriceSnapshot Persistence

Every successful quote refresh creates a `PriceSnapshot` record.

This applies to:

- cache miss with successful external quote response
- cache hit with valid cached quote response

Persisted snapshot purpose:

```text
Keep a local historical record of successful quote refresh results.
```

The snapshot is linked to the active `WatchlistItem`.

Persistence flow:

```text
QuoteRefreshService
      ↓
IPriceSnapshotRepository
      ↓
PriceSnapshotRepository
      ↓
AppDbContext
      ↓
SQLite
```

---

## Watchlist Quote Endpoints

The quote refresh and snapshot listing operations are exposed through the existing watchlist route standard.

Implemented endpoints:

```http
POST /api/watchlist-items/{symbol}/refresh
GET /api/watchlist-items/{symbol}/snapshots
```

These endpoints return DTOs, not Entity models.

Response DTOs:

| Endpoint | DTO |
|---|---|
| `POST /api/watchlist-items/{symbol}/refresh` | `QuoteRefreshResponse` |
| `GET /api/watchlist-items/{symbol}/snapshots` | `List<PriceSnapshotResponse>` |

Reference document:

```text
docs/api-contracts/watchlist-quote-endpoints-api-contract.md
```

---

## Swagger Test Flow

The following flow was verified through Swagger.

### 1. Confirm Active Watchlist Items

```http
GET /api/watchlist-items
```

Expected result:

```text
200 OK
```

If needed, create a symbol such as `AAPL`.

---

### 2. Refresh Active Symbol

```http
POST /api/watchlist-items/AAPL/refresh
```

Expected result:

```text
200 OK
```

Expected response:

```text
QuoteRefreshResponse
```

The first successful refresh should create a PriceSnapshot.

---

### 3. Verify Cache Miss

On first refresh, Redis does not contain the quote key yet.

Expected behavior:

```text
Cache miss
External provider call
Quote cached
PriceSnapshot saved
```

Expected log behavior:

```text
Cache miss for symbol AAPL.
Quote cached for symbol AAPL.
Price snapshot saved for symbol AAPL.
```

---

### 4. Verify Cache Hit

Call refresh again before the 5-minute TTL expires.

```http
POST /api/watchlist-items/AAPL/refresh
```

Expected behavior:

```text
Cache hit
No external provider call needed
PriceSnapshot saved
```

Expected response field:

```json
"isFromCache": true
```

---

### 5. List Saved Snapshots

```http
GET /api/watchlist-items/AAPL/snapshots
```

Expected result:

```text
200 OK
```

Expected response:

```text
List<PriceSnapshotResponse>
```

If the active symbol has no saved snapshots, the endpoint returns:

```json
[]
```

---

### 6. Missing Symbol Verification

```http
POST /api/watchlist-items/ZZZTEST/refresh
GET /api/watchlist-items/ZZZTEST/snapshots
```

Expected result:

```text
404 Not Found
```

Missing symbols do not trigger external API calls.

---

### 7. Inactive Symbol Verification

After soft deleting a symbol, refresh and snapshot listing should return:

```text
404 Not Found
```

Inactive symbols do not trigger external API calls.

---

## Verification Summary

| Area | Status |
|---|---|
| External finance API client | Completed |
| User Secrets setup | Completed |
| Quote provider abstraction | Completed |
| Redis Docker setup | Completed |
| Redis cache-aside flow | Completed |
| Cache hit behavior | Verified |
| Cache miss behavior | Verified |
| Quote refresh service | Completed |
| PriceSnapshot persistence | Completed |
| Snapshot listing endpoint | Completed |
| Swagger endpoint tests | Passed |
| Project build | Passed |

---

## Related Documentation

| Document | Purpose |
|---|---|
| `docs/integrations/public-finance-api-integration.md` | External finance API client flow |
| `docs/configuration/user-secrets-and-strategy-pattern.md` | User Secrets and quote provider setup |
| `docs/cache/redis-cache-design.md` | Redis cache design |
| `docs/api-contracts/quote-refresh-api-contract.md` | Quote refresh service contract |
| `docs/api-contracts/watchlist-quote-endpoints-api-contract.md` | Watchlist quote endpoint contract |

---

## Demo Checklist

Before demo, verify:

- Docker Desktop is running.
- Redis container is running.
- API key exists in User Secrets.
- Project builds successfully.
- Swagger opens successfully.
- Active watchlist symbol exists.
- Refresh endpoint returns `200 OK`.
- First refresh shows cache miss behavior.
- Second refresh shows cache hit behavior.
- PriceSnapshot records are created.
- Snapshot listing endpoint returns saved snapshots.
- GitHub Project Board reflects completed work.

---

## Summary

Week 3 added the quote refresh foundation for the MarketInsight Operations Tracker API.

The implementation now supports:

- external quote retrieval
- secure API key handling
- provider abstraction
- Redis short-term quote caching
- cache-aside refresh behavior
- SQLite PriceSnapshot persistence
- Swagger-testable refresh and snapshot endpoints

The project is ready to move into the next implementation phase, where asynchronous refresh behavior can be introduced.