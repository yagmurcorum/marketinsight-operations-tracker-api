# Logging and Correlation Standard

This document explains the logging and correlation standard used in the MarketInsight Operations Tracker API.

The goal is to make the quote refresh flow observable from application logs without adding production-grade monitoring tools.

---

## Purpose

The project uses logs to make important backend flows easier to follow during development and demo testing.

The main focus of this standard is the quote refresh flow.

The refresh flow includes:

- Watchlist item validation.
- Redis cache lookup.
- External finance API usage.
- Redis cache update.
- PriceSnapshot persistence.
- RabbitMQ async message publishing.
- RabbitMQ async message consumption.
- Background Worker processing.

The logs should help explain what happened, for which symbol it happened, and whether the flow was synchronous or asynchronous.

---

## Why Structured Logging Is Used

Structured logging makes logs easier to search, filter and understand.

Instead of building log messages with string interpolation, the project uses logging placeholders.

Preferred format:

    _logger.LogInformation(
        "Cache hit for symbol {Symbol}.",
        normalizedSymbol);

Avoid this format:

    _logger.LogInformation($"Cache hit for symbol {normalizedSymbol}.");

The placeholder format keeps log properties structured.

This is useful because values such as Symbol, QueueName and CorrelationId can be tracked consistently.

---

## General Logging Rules

The project follows these logging rules:

- Use structured logging placeholders.
- Avoid string interpolation in new logs.
- Include Symbol in quote refresh logs.
- Include QueueName in RabbitMQ queue logs.
- Include CorrelationId in async RabbitMQ flow logs.
- Use LogInformation for successful expected flow events.
- Use LogWarning for recoverable or expected problem cases.
- Use LogError for unexpected failures or failed processing.
- Keep log messages short and specific.
- Do not log secrets, API keys or connection strings.

---

## Log Level Standard

| Log Level | Usage |
|---|---|
| Information | Normal successful flow events |
| Warning | Expected problem cases that do not crash the application |
| Error | Unexpected failures or failed processing |

Examples:

| Scenario | Log Level |
|---|---|
| Price refresh requested | Information |
| Cache hit | Information |
| Cache miss | Information |
| Quote cached | Information |
| PriceSnapshot saved | Information |
| Active watchlist item not found | Warning |
| Quote data could not be retrieved | Warning |
| Invalid RabbitMQ message received | Warning |
| RabbitMQ worker processing failure | Error |

---

## Synchronous Refresh Logging Flow

The synchronous refresh endpoint is:

    POST /api/watchlist-items/{symbol}/refresh

Expected successful cache miss flow:

    Price refresh requested for symbol TSLA.
    Cache miss for symbol TSLA.
    Quote fetched from external API for symbol TSLA.
    Quote cached for symbol TSLA.
    Price snapshot saved for symbol TSLA.

Expected successful cache hit flow:

    Price refresh requested for symbol TSLA.
    Cache hit for symbol TSLA.
    Price snapshot saved for symbol TSLA.

This proves that the refresh flow can be followed from logs.

---

## Redis Cache Logging

Redis cache operations should clearly show whether the requested quote was found in cache.

Expected cache miss log:

    Cache miss for symbol TSLA.

Expected cache hit log:

    Cache hit for symbol TSLA.

Expected cache write log:

    Quote cached for symbol TSLA.

Expected cache deserialization warning:

    Cached quote data could not be deserialized for symbol TSLA.

Cache logs are implemented in:

    src/MarketInsight.Api/Services/Cache/RedisQuoteCacheService.cs

---

## External Quote Provider Logging

When Redis cache does not contain the requested quote, the refresh flow calls the external quote provider.

Expected successful external fetch log:

    Quote fetched from external API for symbol TSLA.

Expected failure log:

    Quote data could not be retrieved for symbol TSLA.

External quote provider success and failure logs are part of the quote refresh flow.

They are implemented in:

    src/MarketInsight.Api/Services/Quotes/QuoteRefreshService.cs

---

## PriceSnapshot Logging

After a quote is resolved from cache or external API, the project stores a PriceSnapshot in SQLite.

Expected successful persistence log:

    Price snapshot saved for symbol TSLA.

This confirms that the refresh operation created a persisted historical price record.

The log is implemented in:

    src/MarketInsight.Api/Services/Quotes/QuoteRefreshService.cs

---

## RabbitMQ Publish Logging

The async refresh endpoint publishes a PriceRefreshMessage to RabbitMQ.

The async refresh endpoint is:

    POST /api/watchlist-items/{symbol}/refresh-async

Expected publish log:

    Async price refresh message published for symbol TSLA with correlation id ...

This log proves that the API accepted the async request and published a message to RabbitMQ.

RabbitMQ publish logging is implemented in:

    src/MarketInsight.Api/Messaging/RabbitMqPriceRefreshPublisher.cs

---

## RabbitMQ Consume Logging

The Background Worker consumes messages from RabbitMQ.

Expected worker startup log:

    Price refresh background worker started consuming queue price-refresh-queue

Expected consume log:

    Async price refresh message consumed for symbol TSLA with correlation id ...

Expected successful processing log:

    Queued price refresh completed for symbol TSLA

Expected skipped processing log:

    Queued price refresh skipped because active watchlist item was not found for symbol TSLA

Expected failed processing log:

    Queued price refresh failed for symbol TSLA. Reason: ...

RabbitMQ consume logging is implemented in:

    src/MarketInsight.Api/Workers/PriceRefreshBackgroundWorker.cs

---

## CorrelationId Standard

CorrelationId is used to connect the async publish and consume flow.

In the current implementation, the async endpoint creates a PriceRefreshMessage with:

    Symbol
    RequestedAtUtc
    CorrelationId

The CorrelationId comes from:

    HttpContext.TraceIdentifier

The publisher sends this CorrelationId with the RabbitMQ message.

The worker reads the same CorrelationId from the message and uses it in the log scope.

This allows the async flow to be followed across:

    refresh-async endpoint
        ↓
    RabbitMQ publish
        ↓
    RabbitMQ queue
        ↓
    Background Worker consume
        ↓
    QuoteRefreshService
        ↓
    PriceSnapshot persistence

---

## Worker Log Scope

The Background Worker creates a log scope while processing a message.

The scope includes:

    CorrelationId
    Symbol

Expected approach:

    using var logScope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = message.CorrelationId,
        ["Symbol"] = message.Symbol
    });

This makes the async processing context available while the message is being handled.

---

## Error Response TraceId Note

The current MIOT-21 scope mentions traceId behavior for unexpected error responses if error middleware is implemented.

At this stage, the project does not require a full production-grade error middleware.

If a global error middleware is added later, unexpected error responses should include a traceId value.

Recommended response shape:

    {
        "message": "An unexpected error occurred.",
        "traceId": "..."
    }

This should use the current HTTP context trace identifier.

Suggested source:

    HttpContext.TraceIdentifier

This keeps runtime errors easier to connect with server-side logs.

---

## What This Issue Does Not Cover

This issue does not add:

- OpenTelemetry.
- Distributed tracing platform.
- Centralized log storage.
- Advanced monitoring dashboard.
- Production-grade logging framework.
- Full global exception handling system.

The current goal is development-friendly observability through consistent structured logs.

---

## Runtime Verification

The logging standard can be verified through Swagger and application console logs.

### Synchronous Cache Miss Test

Request:

    POST /api/watchlist-items/TSLA/refresh

Expected logs when Redis does not contain the quote:

    Price refresh requested for symbol TSLA.
    Cache miss for symbol TSLA.
    Quote fetched from external API for symbol TSLA.
    Quote cached for symbol TSLA.
    Price snapshot saved for symbol TSLA.

### Synchronous Cache Hit Test

Request:

    POST /api/watchlist-items/TSLA/refresh

Expected logs when Redis already contains the quote:

    Price refresh requested for symbol TSLA.
    Cache hit for symbol TSLA.
    Price snapshot saved for symbol TSLA.

### Async Refresh Test

Request:

    POST /api/watchlist-items/TSLA/refresh-async

Expected logs:

    Async price refresh message published for symbol TSLA with correlation id ...
    Async price refresh message consumed for symbol TSLA with correlation id ...
    Price refresh requested for symbol TSLA.
    Cache hit for symbol TSLA.
    Queued price refresh completed for symbol TSLA

If the cache has expired or was cleared, the async flow may show cache miss and external API logs instead.

That is also valid.

---

## Current Implementation Files

The current logging and correlation implementation is mainly located in:

    src/MarketInsight.Api/Services/Quotes/QuoteRefreshService.cs
    src/MarketInsight.Api/Services/Cache/RedisQuoteCacheService.cs
    src/MarketInsight.Api/Messaging/RabbitMqPriceRefreshPublisher.cs
    src/MarketInsight.Api/Workers/PriceRefreshBackgroundWorker.cs

Related message contract:

    src/MarketInsight.Api/Messaging/PriceRefreshMessage.cs

Related async endpoint:

    src/MarketInsight.Api/Controllers/WatchlistItemsController.cs

---

## Implementation Outcome

After this implementation:

- Structured logging placeholders are used.
- String interpolation is avoided in new logs.
- Refresh flow logs are consistent.
- Cache hit log is visible.
- Cache miss log is visible.
- Quote cached log is visible.
- External API quote fetch log is visible.
- PriceSnapshot saved log is visible.
- RabbitMQ publish log is visible.
- RabbitMQ consume log is visible.
- CorrelationId is included in the async message flow.
- Worker logs use CorrelationId scope.
- Logging standard is documented.
- Project build completed successfully.

---

## Review Questions

1. Why do we use structured logging placeholders?
2. Why should new logs avoid string interpolation?
3. Which log proves that Redis cache was used?
4. Which log proves that external API was called?
5. Which log proves that a PriceSnapshot was saved?
6. Which log proves that RabbitMQ publish worked?
7. Which log proves that RabbitMQ consume worked?
8. Why is CorrelationId useful in the async flow?
9. Where does the current CorrelationId come from?
10. Why are OpenTelemetry and centralized log storage out of scope for this MVP?

---

## Summary

The logging standard makes the refresh flow easier to observe from application logs.

The project now shows important events for:

- Quote refresh request.
- Redis cache hit.
- Redis cache miss.
- External API quote fetch.
- Redis cache write.
- PriceSnapshot persistence.
- RabbitMQ publish.
- RabbitMQ consume.
- Background Worker processing.

The async RabbitMQ flow carries CorrelationId from the HTTP request into the message and then into the worker log scope.

This keeps the project simple, demo-friendly and understandable while still showing the core idea of observability.