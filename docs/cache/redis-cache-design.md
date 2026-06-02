# Redis Cache Design

This document describes how Redis is used as a short-term quote cache in the MarketInsight Operations Tracker API.

The purpose of this document is to define the Redis cache responsibility, cache-aside behavior, key format, TTL standard, and local Docker Compose setup for the quote refresh flow.

---

## Purpose

The purpose of Redis in this project is to cache recently retrieved quote data for a short period of time.

Redis is used to reduce repeated external finance API calls during quote refresh operations.

Redis is not used as persistent storage.

SQLite remains the persistent database for saved application data such as WatchlistItem and PriceSnapshot records.

---

## Current Scope

This implementation includes:

- Redis service in `docker-compose.yml`
- Redis connection configuration
- `IQuoteCacheService`
- `RedisQuoteCacheService`
- Cache-aside pattern
- `quote:{symbol}` key format
- 5-minute cache TTL
- Basic JSON serialization and deserialization

This implementation does not include:

- RabbitMQ
- Background Worker
- Cache management endpoints
- Redis persistence
- Distributed cache invalidation
- Production Redis hardening

---

## Redis Responsibility

Redis is responsible only for short-term quote caching.

| Responsibility | Redis |
|---|---|
| Store recently retrieved quote data | Yes |
| Reduce repeated external API calls | Yes |
| Store permanent quote history | No |
| Replace SQLite | No |
| Store PriceSnapshot records | No |
| Store WatchlistItem records | No |

Correct mindset:

```text
Redis = short-term cache
SQLite = persistent storage
```

---

## Docker Compose Setup

Redis is defined in the root-level `docker-compose.yml` file.

Expected location:

```text
docker-compose.yml
```

Redis service definition:

```yaml
services:
  redis:
    image: redis:7-alpine
    container_name: marketinsight-redis
    ports:
      - "6379:6379"
    restart: unless-stopped
```

Redis can be started with:

```powershell
docker compose up -d redis
```

Redis can be verified with:

```powershell
docker exec -it marketinsight-redis redis-cli ping
```

Expected result:

```text
PONG
```

---

## Application Configuration

The Redis connection string is stored in `appsettings.json`.

Expected configuration:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

This value is not a secret and can be committed.

---

## Cache Service Files

Redis cache behavior is isolated behind a service abstraction.

Implemented files:

```text
src/MarketInsight.Api/Services/Cache/IQuoteCacheService.cs
src/MarketInsight.Api/Services/Cache/RedisQuoteCacheService.cs
```

`IQuoteCacheService` defines the cache contract.

`RedisQuoteCacheService` implements Redis-based quote caching.

---

## Cache Key Standard

Quote cache keys must follow this format:

```text
quote:{symbol}
```

Example:

```text
quote:AAPL
```

Rules:

- Symbol values are normalized before building the key.
- Symbols are trimmed.
- Symbols are converted to uppercase.
- The key prefix is always `quote:`.

Reason:

```text
AAPL, aapl, and Aapl should resolve to the same cache key.
```

---

## TTL Standard

Quote cache entries use a 5-minute TTL.

| Setting | Value |
|---|---|
| TTL | 5 minutes |
| Purpose | Keep quote data fresh enough for local demo and testing |
| Persistence | Not permanent |

Reason:

```text
Quote data changes frequently, so cached values should expire quickly.
```

---

## Cache-Aside Pattern

The project uses cache-aside pattern.

Expected flow:

```text
Quote refresh requested
      ↓
Check Redis cache by quote:{symbol}
      ↓
If cache hit:
      return cached quote data
      ↓
If cache miss:
      call external finance provider
      ↓
Store successful quote response in Redis
      ↓
Return quote response
```

Redis does not automatically fetch data.

The application controls when data is read from or written to cache.

---

## Serialization Behavior

Quote data is stored in Redis as serialized JSON.

Stored model:

```text
QuoteResponse
```

Behavior:

- `QuoteResponse` is serialized before writing to Redis.
- Cached JSON is deserialized back into `QuoteResponse`.
- If deserialization fails, the invalid cache key is deleted.
- The application treats invalid cached data as a cache miss.

This keeps cache behavior safe and simple.

---

## Dependency Injection

Redis connection and cache service are registered in `Program.cs`.

Expected registrations:

```csharp
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException("Redis connection string is not configured.");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<IQuoteCacheService, RedisQuoteCacheService>();
```

`IConnectionMultiplexer` is registered as singleton because Redis connections are intended to be reused.

`IQuoteCacheService` is registered as scoped to match the application service usage pattern.

---

## Logging

Redis cache service should log important cache behavior.

Expected log messages:

```text
Cache miss for symbol AAPL.
Cache hit for symbol AAPL.
Quote cached for symbol AAPL.
```

These logs will help verify cache behavior during Swagger testing after the refresh endpoint is implemented.

---

## Expected Future Usage

The Redis cache service is prepared for future use by `QuoteRefreshService`.

Expected future flow:

```text
QuoteRefreshService
      ↓
IQuoteCacheService
      ↓
RedisQuoteCacheService
      ↓
Redis
```

At this stage, the cache service is not exposed through a Controller.

It will be used when the quote refresh use case is implemented.

---

## Out of Scope

This document does not cover:

- RabbitMQ
- Background Worker
- Async refresh
- Cache management endpoints
- PriceSnapshot persistence
- Refresh endpoint implementation
- Snapshot listing endpoint
- Production Redis security
- Redis persistence configuration
- Advanced retry or failover behavior

These topics will be handled in later issues if needed.

---

## Verification

To verify Redis container setup:

```powershell
docker compose up -d redis
docker ps
docker exec -it marketinsight-redis redis-cli ping
```

Expected Redis response:

```text
PONG
```

To verify API build:

```powershell
dotnet build
```

Expected result:

```text
Build succeeded.
```

No Swagger endpoint is expected for this issue because cache behavior is not connected to an API endpoint yet.

---

## Review Checklist

Before closing this issue, check:

- `docker-compose.yml` contains Redis service.
- Redis container starts locally.
- Redis responds with `PONG`.
- `Redis:ConnectionString` exists in `appsettings.json`.
- `StackExchange.Redis` package is installed.
- `IQuoteCacheService` exists.
- `RedisQuoteCacheService` exists.
- Cache key format is `quote:{symbol}`.
- Cache TTL is 5 minutes.
- Quote data is serialized and deserialized safely.
- Redis is documented as short-term cache only.
- SQLite remains the persistent storage.
- Project builds successfully.

---

## Summary

This issue adds Redis cache infrastructure for quote data.

Implemented decisions:

- Redis runs locally through Docker Compose.
- Redis connection string is configured in `appsettings.json`.
- Quote cache behavior is isolated behind `IQuoteCacheService`.
- `RedisQuoteCacheService` uses Redis through `StackExchange.Redis`.
- Cache keys use the `quote:{symbol}` format.
- Quote cache entries expire after 5 minutes.
- Redis is used only as short-term cache.
- SQLite remains the persistent database.

The next step is to implement the quote refresh service and persist successful refresh results as PriceSnapshot records.