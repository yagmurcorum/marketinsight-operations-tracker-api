# Service Layer and Quote Refresh Flow

This document defines the service boundary and architecture flow for the quote refresh feature in the MarketInsight Operations Tracker API.

The purpose of this standard is to keep the Week 3 quote refresh flow aligned with the existing layered monolith architecture before implementing external finance API integration, Redis cache, PriceSnapshot persistence, and snapshot listing.

---

## Purpose

The purpose of this document is to define where quote refresh responsibilities belong in the current application architecture.

The quote refresh feature introduces responsibilities that do not belong directly inside the existing Watchlist CRUD service:

- Retrieving quote data from an external finance API
- Using Redis as a short-term quote cache
- Saving successful refresh results as PriceSnapshot records
- Listing saved PriceSnapshot records for a watchlist symbol
- Keeping Watchlist CRUD responsibilities separate from refresh orchestration

This document prevents the refresh flow from being mixed into the wrong layers.

---

## Architecture Baseline

The project follows a learning-focused layered monolith architecture.

Current expected flow:

```text
Client / Swagger
      ↓
Controller
      ↓
Service
      ↓
Repository
      ↓
AppDbContext / EF Core
      ↓
SQLite Database
```

Core layer rules:

- Controller handles HTTP request and response flow.
- Service handles use cases and business rules.
- Repository handles data access.
- Entity represents database persistence model.
- DTO represents API request and response contract.

This structure must be preserved while adding quote refresh functionality.

---

## Current Watchlist CRUD Flow

The current Watchlist CRUD flow is handled through:

```text
Client / Swagger
      ↓
WatchlistItemsController
      ↓
IWatchlistItemService
      ↓
WatchlistItemService
      ↓
IWatchlistItemRepository
      ↓
WatchlistItemRepository
      ↓
AppDbContext / EF Core
      ↓
SQLite Database
```

The WatchlistItemService is responsible for:

- Listing active watchlist items
- Getting a watchlist item by symbol
- Creating a new watchlist item
- Detecting duplicate active symbols
- Reactivating inactive symbols
- Soft deleting active symbols
- Mapping WatchlistItem entities to WatchlistItemResponse DTOs

This service should remain focused on WatchlistItem CRUD and lifecycle behavior.

---

## Why Quote Refresh Needs a Separate Service

Quote refresh is not a basic Watchlist CRUD operation.

It is a separate use case because it coordinates:

- Active WatchlistItem validation
- Redis cache lookup
- External finance API quote retrieval
- Quote provider abstraction
- PriceSnapshot persistence
- Refresh response mapping
- Refresh-related logging

If this logic is added to WatchlistItemService, that service becomes responsible for both Watchlist CRUD and quote refresh orchestration.

Incorrect direction:

```text
WatchlistItemService
      ↓
CRUD logic
Refresh logic
External API logic
Redis cache logic
PriceSnapshot persistence logic
```

Preferred direction:

```text
WatchlistItemService
      ↓
Watchlist CRUD use cases

QuoteRefreshService
      ↓
Quote refresh use case
```

This keeps each service focused on one application responsibility.

---

## Service Boundary Decision

The quote refresh use case should be handled by a separate service:

```csharp
IQuoteRefreshService
QuoteRefreshService
```

The WatchlistItemService should not own quote refresh logic.

The QuoteRefreshService should coordinate the refresh workflow and use repository, cache, and provider abstractions.

---

## Quote Refresh Architecture Flow

The synchronous quote refresh flow should follow this structure:

```text
Client / Swagger
      ↓
WatchlistItemsController
      ↓
IQuoteRefreshService
      ↓
QuoteRefreshService
      ↓
IWatchlistItemRepository
      ↓
IQuoteCacheService
      ↓
IQuoteProvider
      ↓
IFinanceQuoteClient
      ↓
IPriceSnapshotRepository
      ↓
AppDbContext / EF Core
      ↓
SQLite Database
```

The Controller receives the HTTP request.

The QuoteRefreshService owns the refresh use case.

Repositories handle database access.

The cache service isolates Redis behavior.

The quote provider hides external finance API details.

SQLite remains the persistent storage.

---

## Layer Responsibility Table

| Layer | Responsibility in Quote Refresh Flow | Should Not Do |
|---|---|---|
| `WatchlistItemsController` | Exposes refresh and snapshot endpoints | Should not call Redis, AppDbContext, or external APIs directly |
| `QuoteRefreshService` | Coordinates the refresh use case | Should not return HTTP-specific objects such as `Ok()` or `NotFound()` |
| `WatchlistItemRepository` | Reads WatchlistItem data from SQLite | Should not decide refresh business rules |
| `QuoteCacheService` | Reads and writes quote data in Redis | Should not persist long-term data |
| `QuoteProvider` | Provides quote data through an abstraction | Should not expose external API details to the Controller |
| `FinanceQuoteClient` | Calls the external finance API | Should not contain WatchlistItem business rules |
| `PriceSnapshotRepository` | Saves and lists PriceSnapshot records | Should not decide whether refresh is allowed |
| `DTO` | Shapes API response contracts | Should not represent database persistence models |

---

## WatchlistItemService Responsibility

WatchlistItemService remains responsible for WatchlistItem CRUD use cases.

It should handle:

- List active watchlist items
- Get watchlist item by symbol
- Create watchlist item
- Detect active duplicate symbols
- Reactivate inactive symbols
- Soft delete active watchlist items
- Map WatchlistItem entities to WatchlistItemResponse DTOs

Correct responsibility:

```text
WatchlistItemService = WatchlistItem CRUD and lifecycle rules
```

WatchlistItemService should not:

- Call external finance APIs
- Read from or write to Redis
- Save PriceSnapshot records
- Handle quote refresh orchestration
- Contain RabbitMQ or Background Worker logic

---

## QuoteRefreshService Responsibility

QuoteRefreshService should handle the synchronous quote refresh use case.

It should:

- Normalize the requested symbol
- Check whether the WatchlistItem exists
- Check whether the WatchlistItem is active
- Prevent refresh for missing or inactive symbols
- Check Redis cache using `quote:{symbol}`
- Use cached quote data when available
- Call the quote provider when cache is missing
- Cache successful quote data with a short TTL
- Save successful refresh results as PriceSnapshot records
- Return a QuoteRefreshResponse DTO

Correct responsibility:

```text
QuoteRefreshService = quote refresh orchestration
```

QuoteRefreshService should not:

- Define API routes
- Return HTTP-specific response objects
- Directly expose external API response models
- Directly expose Entity models to the Controller
- Store API keys
- Replace the repository layer
- Use Redis as persistent storage
- Contain RabbitMQ consumer logic

---

## Repository Decisions

### WatchlistItemRepository

The existing WatchlistItemRepository should be used to check whether the requested symbol exists and is active.

Expected repository need:

```csharp
GetByNormalizedSymbolAsync
```

Reason:

```text
QuoteRefreshService must know whether the symbol exists and whether it is active before refreshing quote data.
```

The Repository retrieves the data.

The Service decides the business outcome.

### PriceSnapshotRepository

A new repository should be added for PriceSnapshot persistence and listing:

```text
src/MarketInsight.Api/Repositories/IPriceSnapshotRepository.cs
src/MarketInsight.Api/Repositories/PriceSnapshotRepository.cs
```

PriceSnapshotRepository should handle:

- Saving new PriceSnapshot records
- Listing PriceSnapshot records for a WatchlistItem or symbol
- Using EF Core LINQ queries
- Accessing AppDbContext
- Returning Entity models to the Service layer

PriceSnapshotRepository should not:

- Decide whether a symbol can be refreshed
- Normalize symbols as a business rule
- Call external finance APIs
- Read or write Redis cache
- Return HTTP status codes
- Return API response DTOs directly

---

## Cache and Provider Boundary

Redis and external finance API logic should be isolated behind abstractions.

Expected cache service:

```text
src/MarketInsight.Api/Services/Cache/IQuoteCacheService.cs
src/MarketInsight.Api/Services/Cache/RedisQuoteCacheService.cs
```

Expected quote provider:

```text
src/MarketInsight.Api/Providers/Quotes/IQuoteProvider.cs
src/MarketInsight.Api/Providers/Quotes/FinnhubQuoteProvider.cs
```

Expected finance client:

```text
src/MarketInsight.Api/Clients/Finance/IFinanceQuoteClient.cs
src/MarketInsight.Api/Clients/Finance/FinanceQuoteClient.cs
```

Cache standard:

| Rule | Value |
|---|---|
| Pattern | Cache-aside |
| Key format | `quote:{symbol}` |
| Example key | `quote:AAPL` |
| TTL | 5 minutes |
| Purpose | Short-term quote cache |

Redis should not be treated as persistent storage.

---

## DTO Decision

Quote refresh should use DTOs for API response shape.

Expected future DTOs:

```text
src/MarketInsight.Api/DTOs/Quotes/QuoteResponse.cs
src/MarketInsight.Api/DTOs/Quotes/QuoteRefreshResponse.cs
src/MarketInsight.Api/DTOs/Quotes/PriceSnapshotResponse.cs
```

DTOs should:

- Represent API response contracts
- Hide database-specific fields when needed
- Avoid exposing external API response models directly
- Keep Swagger response shape understandable

Entities should not be returned directly from refresh or snapshot endpoints.

---

## Route Decision

The quote refresh endpoints should stay under the current WatchlistItem route family:

```text
/api/watchlist-items
```

Selected Week 3 routes:

| HTTP Method | Route | Purpose |
|---|---|---|
| POST | `/api/watchlist-items/{symbol}/refresh` | Refresh quote data synchronously for an active watchlist symbol |
| GET | `/api/watchlist-items/{symbol}/snapshots` | List saved price snapshots for an active watchlist symbol |

These routes are selected because the refresh and snapshot operations belong to a specific WatchlistItem symbol.

Avoid:

```text
POST /api/watchlist/{symbol}/refresh
GET /api/watchlist/{symbol}/snapshots
POST /api/quotes/{symbol}/refresh
GET /api/price-snapshots/{symbol}
```

The current MVP route standard is:

```text
/api/watchlist-items
```

---

## Why Refresh Uses POST

The refresh endpoint uses POST:

```text
POST /api/watchlist-items/{symbol}/refresh
```

Reason:

- It starts a server-side operation.
- It may call an external finance API.
- It may write quote data to Redis.
- It may save a PriceSnapshot to SQLite.
- It is not a simple read-only operation.

This endpoint is a domain action / command endpoint.

It is not a classic WatchlistItem metadata update endpoint.

Therefore, it should not be modeled as:

```text
PUT /api/watchlist-items/{symbol}
PATCH /api/watchlist-items/{symbol}
```

---

## Synchronous Quote Refresh Flow

Expected synchronous refresh behavior:

```text
Client sends POST /api/watchlist-items/{symbol}/refresh
      ↓
WatchlistItemsController receives request
      ↓
Controller calls QuoteRefreshService
      ↓
QuoteRefreshService normalizes symbol
      ↓
QuoteRefreshService checks active WatchlistItem
      ↓
If symbol is missing or inactive:
      return application-level not found result
      ↓
If symbol is active:
      check Redis cache with quote:{symbol}
      ↓
If cache hit:
      use cached quote data
      ↓
If cache miss:
      call QuoteProvider
      ↓
QuoteProvider uses FinanceQuoteClient
      ↓
FinanceQuoteClient calls external finance API
      ↓
QuoteRefreshService caches successful quote data
      ↓
QuoteRefreshService saves PriceSnapshot through PriceSnapshotRepository
      ↓
QuoteRefreshService returns QuoteRefreshResponse DTO
      ↓
Controller maps result to HTTP response
```

Expected response behavior:

| Situation | Expected HTTP Response |
|---|---|
| Successful refresh | `200 OK` |
| Missing symbol | `404 Not Found` |
| Inactive symbol | `404 Not Found` |
| External API failure | Controlled error response |

Missing or inactive symbols should not trigger external API calls, Redis writes, or PriceSnapshot persistence.

---

## Snapshot Listing Flow

Snapshot listing should also stay under the WatchlistItem route family.

Endpoint:

```text
GET /api/watchlist-items/{symbol}/snapshots
```

Expected behavior:

```text
Client sends GET /api/watchlist-items/{symbol}/snapshots
      ↓
WatchlistItemsController receives request
      ↓
Controller calls QuoteRefreshService or snapshot listing service method
      ↓
Service normalizes symbol
      ↓
Service checks active WatchlistItem
      ↓
If missing or inactive:
      return application-level not found result
      ↓
If active:
      read snapshots through PriceSnapshotRepository
      ↓
Map PriceSnapshot entities to PriceSnapshotResponse DTOs
      ↓
Controller returns HTTP response
```

Expected response behavior:

| Situation | Expected HTTP Response |
|---|---|
| Active symbol with snapshots | `200 OK` with snapshot list |
| Active symbol without snapshots | `200 OK` with empty list |
| Missing symbol | `404 Not Found` |
| Inactive symbol | `404 Not Found` |

---

## Controller Responsibility in Refresh Flow

WatchlistItemsController should expose the endpoints and map service results to HTTP responses.

It should:

- Define route attributes
- Receive the symbol route parameter
- Call IQuoteRefreshService
- Return proper HTTP status codes
- Return DTO responses
- Add XML Summary comments
- Add Swagger response metadata

Expected future methods:

```csharp
RefreshWatchlistItemQuoteAsync
GetWatchlistItemSnapshotsAsync
```

The Controller should not:

- Call AppDbContext directly
- Call Redis directly
- Call HttpClient directly
- Call external finance API directly
- Save PriceSnapshot records directly
- Return Entity models directly

---

## Dependency Direction

The dependency direction should remain clear.

Preferred direction:

```text
WatchlistItemsController
      depends on IQuoteRefreshService

QuoteRefreshService
      depends on:
          IWatchlistItemRepository
          IQuoteCacheService
          IQuoteProvider
          IPriceSnapshotRepository

QuoteProvider
      depends on IFinanceQuoteClient

Repositories
      depend on AppDbContext
```

Avoid:

```text
Controller depends on AppDbContext
Controller depends on Redis
Controller depends on HttpClient
Repository depends on Controller
Repository depends on DTOs
Entity depends on Service
DTO depends on EF Core
```

---

## What This Standard Protects

This standard protects the project from:

- Fat controllers
- WatchlistItemService becoming too large
- Business logic leaking into repositories
- External API logic leaking into controllers
- Redis logic being scattered across the application
- PriceSnapshot persistence being handled without a repository
- Entity models being returned directly from API endpoints
- Route naming inconsistency
- Confusing Week 3 and Week 4 responsibilities
- Moving RabbitMQ or Background Worker logic into Week 3 too early

---

## Out of Scope

This document does not implement:

- Public finance API client
- User Secrets configuration
- Quote provider implementation
- Redis cache service
- Docker Compose setup
- QuoteRefreshService code
- PriceSnapshotRepository code
- Refresh endpoint code
- Snapshot listing endpoint code
- RabbitMQ async refresh
- Background Worker consumer
- Alert evaluation logic
- Authentication or user-specific watchlists

These items will be handled in later implementation issues.

---

## Week 3 Implementation Direction

The Week 3 implementation should follow this order:

```text
1. Define service boundary and architecture flow
2. Add public finance API client foundation
3. Configure User Secrets and quote provider strategy
4. Add Redis cache service with Docker Compose setup
5. Add QuoteRefreshService and PriceSnapshot persistence
6. Add refresh and snapshot listing endpoints
7. Complete Week 3 documentation and Swagger test notes
```

This order keeps the implementation beginner-friendly and prevents architecture drift.

---

## Review Checklist

Before implementing or reviewing the quote refresh flow, check:

- Does WatchlistItemService remain focused on Watchlist CRUD?
- Is quote refresh handled by QuoteRefreshService?
- Does the Controller only handle HTTP request and response flow?
- Does the Controller avoid direct AppDbContext usage?
- Does the Controller avoid direct Redis usage?
- Does the Controller avoid direct external API calls?
- Does QuoteRefreshService check active WatchlistItem before refresh?
- Does QuoteRefreshService avoid refreshing missing symbols?
- Does QuoteRefreshService avoid refreshing inactive symbols?
- Is Redis used only as short-term quote cache?
- Is SQLite used as persistent storage?
- Is PriceSnapshot persistence handled through PriceSnapshotRepository?
- Are Entity models avoided as direct API responses?
- Are DTOs used for refresh and snapshot response contracts?
- Are routes under `/api/watchlist-items`?
- Is RabbitMQ kept out of Week 3 synchronous refresh scope?

---

## Summary

The quote refresh feature should be added as a separate use case in the service layer.

Core decisions:

- WatchlistItemService remains responsible for Watchlist CRUD.
- QuoteRefreshService owns the quote refresh workflow.
- WatchlistItemsController exposes refresh and snapshot endpoints.
- WatchlistItemRepository validates active symbol existence.
- QuoteCacheService isolates Redis behavior.
- QuoteProvider isolates external finance API provider behavior.
- PriceSnapshotRepository handles snapshot persistence and listing.
- Redis is used only for short-term quote cache.
- SQLite remains the persistent database.
- API responses should use DTOs, not Entity models.
- Routes should stay under `/api/watchlist-items`.

Selected Week 3 routes:

```text
POST /api/watchlist-items/{symbol}/refresh
GET  /api/watchlist-items/{symbol}/snapshots
```

This boundary keeps the project aligned with the existing layered architecture while preparing the system for Redis, PriceSnapshot persistence, snapshot listing, and future Week 4 async refresh reuse.