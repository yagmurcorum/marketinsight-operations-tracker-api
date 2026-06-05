# Week 4 Async Processing, Logging and Observability Summary

This document summarizes the Week 4 implementation for RabbitMQ asynchronous refresh processing, Background Worker message consumption, structured logging, correlation, basic observability endpoints, and Swagger-based verification.

The goal of this document is to make the Week 4 work reviewable and demo-ready.

---

## Scope

Week 4 focused on asynchronous processing and basic observability.

Completed areas:

- RabbitMQ Docker Compose setup
- RabbitMQ configuration through appsettings
- async price refresh message contract
- RabbitMQ publisher implementation
- async refresh endpoint
- Background Worker implementation
- queued message consumption
- reuse of QuoteRefreshService from the worker
- structured logging alignment
- CorrelationId usage in async flow
- basic health and dependency status verification
- observability summary endpoint
- Swagger-based endpoint verification
- final MVP verification preparation

Out of scope:

- advanced retry policy
- dead-letter queue
- separate worker service project
- production monitoring platform
- OpenTelemetry setup
- metrics dashboard
- alert evaluation endpoint
- ActionItem creation flow
- authentication

---

## RabbitMQ Setup

RabbitMQ is used for asynchronous price refresh processing.

RabbitMQ was added to the local Docker Compose setup.

Local RabbitMQ Management UI:

    http://localhost:15672

Default local credentials:

    username: guest
    password: guest

Main queue:

    price-refresh-queue

RabbitMQ is used only for asynchronous queue-based processing.

It does not replace SQLite persistence.

It does not replace Redis cache.

---

## RabbitMQ Configuration

RabbitMQ configuration is stored under the RabbitMq configuration section.

Configuration values:

| Setting | Purpose |
|---|---|
| HostName | RabbitMQ host |
| Port | RabbitMQ AMQP port |
| UserName | RabbitMQ username |
| Password | RabbitMQ password |
| QueueName | Queue used for async price refresh messages |

Current queue name:

    price-refresh-queue

Configuration model:

    RabbitMqOptions

Reference document:

    docs/messaging/rabbitmq-async-processing.md

---

## Async Price Refresh Message

The async refresh flow uses a message contract named:

    PriceRefreshMessage

Message fields:

| Field | Purpose |
|---|---|
| Symbol | Symbol requested for refresh |
| RequestedAtUtc | UTC time when the async request was created |
| CorrelationId | Identifier used to connect publish and consume logs |

Purpose of the message:

    Carry the minimum information needed for the worker to process an async quote refresh request.

The message should stay simple and beginner-friendly.

---

## RabbitMQ Publisher

The RabbitMQ publisher is responsible for publishing async price refresh messages to RabbitMQ.

Implemented components:

    IPriceRefreshPublisher
    RabbitMqPriceRefreshPublisher

Publisher responsibilities:

- validate message Symbol
- validate message CorrelationId
- connect to RabbitMQ
- declare the queue
- serialize PriceRefreshMessage
- publish message to price-refresh-queue
- mark the message as persistent
- include CorrelationId in RabbitMQ message properties
- log successful publish operation

Expected publish log:

    Async price refresh message published for symbol MSFT with correlation id ...

Reference document:

    docs/messaging/rabbitmq-async-processing.md

---

## Async Refresh Endpoint

The async refresh endpoint is exposed through the existing watchlist route standard.

Implemented endpoint:

    POST /api/watchlist-items/{symbol}/refresh-async

Expected successful response:

    202 Accepted

Expected response body:

    {
        "message": "Price refresh request accepted.",
        "symbol": "MSFT",
        "status": "Queued"
    }

Endpoint behavior:

- normalizes the requested symbol
- verifies that the symbol exists as an active WatchlistItem
- creates a PriceRefreshMessage
- uses HttpContext.TraceIdentifier as CorrelationId
- publishes the message to RabbitMQ
- returns 202 Accepted

If the symbol does not exist or is inactive, the endpoint returns:

    404 Not Found

Missing or inactive symbols should not publish RabbitMQ messages.

---

## Background Worker

The Background Worker consumes queued price refresh messages from RabbitMQ.

Implemented worker:

    PriceRefreshBackgroundWorker

Worker responsibilities:

- connect to RabbitMQ
- declare the queue
- consume messages from price-refresh-queue
- deserialize PriceRefreshMessage
- read Symbol and CorrelationId
- create log scope with Symbol and CorrelationId
- resolve IQuoteRefreshService through IServiceScopeFactory
- call QuoteRefreshService.RefreshQuoteAsync
- acknowledge successful messages
- log failures

Reference document:

    docs/messaging/background-worker-design.md

---

## Worker and Service Responsibility

The worker does not contain quote refresh business logic.

The worker only coordinates async message processing.

Business logic remains inside:

    QuoteRefreshService

This means both sync and async refresh flows reuse the same refresh rules.

Sync flow:

    POST /api/watchlist-items/{symbol}/refresh
        ↓
    QuoteRefreshService

Async flow:

    POST /api/watchlist-items/{symbol}/refresh-async
        ↓
    RabbitMQ
        ↓
    PriceRefreshBackgroundWorker
        ↓
    QuoteRefreshService

This keeps the refresh logic in one place.

---

## Async Refresh Flow

The completed async refresh flow is:

    Swagger / Client
        ↓
    POST /api/watchlist-items/{symbol}/refresh-async
        ↓
    WatchlistItemsController
        ↓
    RabbitMqPriceRefreshPublisher
        ↓
    RabbitMQ price-refresh-queue
        ↓
    PriceRefreshBackgroundWorker
        ↓
    QuoteRefreshService
        ↓
    Redis / External Finance API
        ↓
    PriceSnapshotRepository
        ↓
    SQLite

This flow verifies that async processing can still create a persistent PriceSnapshot.

---

## RabbitMQ Queue Verification

RabbitMQ Management UI was used to verify message processing.

Queue:

    price-refresh-queue

Expected queue state after worker processing:

    Ready: 0
    Unacked: 0

Meaning:

| Value | Meaning |
|---|---|
| Ready: 0 | No waiting messages remain in the queue |
| Unacked: 0 | No message is currently stuck in unacknowledged state |

This confirms that the worker consumed and acknowledged the queued message.

---

## Structured Logging

Week 4 aligned logs around structured logging placeholders.

Preferred format:

    _logger.LogInformation(
        "Cache hit for symbol {Symbol}.",
        normalizedSymbol);

Avoided format:

    _logger.LogInformation($"Cache hit for symbol {normalizedSymbol}.");

Structured logging keeps values such as Symbol, QueueName and CorrelationId visible and consistent.

Reference document:

    docs/observability/logging-standard.md

---

## Quote Refresh Logging

The quote refresh flow now includes visible logs for key events.

Expected cache miss logs:

    Price refresh requested for symbol MSFT.
    Cache miss for symbol MSFT.
    Quote fetched from external API for symbol MSFT.
    Quote cached for symbol MSFT.
    Price snapshot saved for symbol MSFT.

Expected cache hit logs:

    Price refresh requested for symbol MSFT.
    Cache hit for symbol MSFT.
    Price snapshot saved for symbol MSFT.

These logs make the refresh flow easier to explain during demo.

---

## RabbitMQ Logging

The async RabbitMQ flow includes publish and consume logs.

Expected publish log:

    Async price refresh message published for symbol MSFT with correlation id ...

Expected consume log:

    Async price refresh message consumed for symbol MSFT with correlation id ...

Expected completion log:

    Queued price refresh completed for symbol MSFT

These logs show that:

- the async endpoint published a message
- RabbitMQ received the message
- the worker consumed the message
- the refresh flow completed after async processing

---

## CorrelationId Usage

CorrelationId is used to connect the async publish and consume flow.

Current CorrelationId source:

    HttpContext.TraceIdentifier

CorrelationId is included in:

- PriceRefreshMessage
- RabbitMQ message properties
- publish log
- worker consume log
- worker log scope

Purpose:

    Make the async flow easier to trace from logs.

The CorrelationId connects:

    HTTP request
        ↓
    RabbitMQ publish
        ↓
    RabbitMQ consume
        ↓
    Background Worker processing

---

## Basic Observability

Week 4 added basic observability endpoints.

Implemented endpoints:

    GET /api/health
    GET /api/system/info
    GET /api/system/dependencies
    GET /api/observability/summary

Purpose:

| Endpoint | Purpose |
|---|---|
| GET /api/health | Confirms that the API is running |
| GET /api/system/info | Returns application name, version and environment |
| GET /api/system/dependencies | Checks SQLite, Redis, RabbitMQ and external finance API |
| GET /api/observability/summary | Returns available observability features |

Reference document:

    docs/observability/basic-observability.md

---

## Dependency Status Endpoint

The dependency status endpoint is:

    GET /api/system/dependencies

Checked dependencies:

- SQLite
- Redis
- RabbitMQ
- ExternalFinanceApi

Status values:

| Status | Meaning |
|---|---|
| Healthy | Dependency is available |
| Unhealthy | Dependency check failed |
| NotConfigured | Required configuration is missing |

The external finance API check returns NotConfigured when the API key is missing.

The external finance API check uses a short timeout.

Current timeout:

    3 seconds

This keeps the endpoint responsive during local testing.

---

## Observability Summary Endpoint

The observability summary endpoint is:

    GET /api/observability/summary

Expected response includes:

- applicationName
- environment
- generatedAtUtc
- summary
- endpoints
- observabilityFeatures

Purpose:

    Show a beginner-friendly summary of available observability features.

This endpoint helps explain the project during final demo.

---

## Swagger Verification Flow

The following flow was verified through Swagger.

### 1. Health Verification

Request:

    GET /api/health

Expected result:

    200 OK

---

### 2. System Info Verification

Request:

    GET /api/system/info

Expected result:

    200 OK

---

### 3. Dependency Status Verification

Request:

    GET /api/system/dependencies

Expected result:

    200 OK

Expected dependencies:

- SQLite
- Redis
- RabbitMQ
- ExternalFinanceApi

---

### 4. Watchlist Verification

Request:

    GET /api/watchlist-items

Expected result:

    200 OK

---

### 5. Sync Refresh Verification

Request:

    POST /api/watchlist-items/MSFT/refresh

Expected result:

    200 OK

Expected behavior:

- cache miss can be verified after deleting Redis key
- cache hit can be verified by calling refresh again before TTL expires
- PriceSnapshot is saved after successful refresh

---

### 6. Snapshot Listing Verification

Request:

    GET /api/watchlist-items/MSFT/snapshots

Expected result:

    200 OK

Expected response:

    List<PriceSnapshotResponse>

---

### 7. Async Refresh Verification

Request:

    POST /api/watchlist-items/MSFT/refresh-async

Expected result:

    202 Accepted

Expected behavior:

- RabbitMQ publish log appears
- worker consume log appears
- queued refresh completed log appears
- new PriceSnapshot is created

---

### 8. Observability Summary Verification

Request:

    GET /api/observability/summary

Expected result:

    200 OK

Expected response:

    ObservabilitySummaryResponse

---

## XML Summary and Swagger

Week 4 endpoints include XML Summary comments and Swagger response metadata.

Swagger was used to verify that endpoint descriptions are visible.

Important verified endpoints:

- GET /api/health
- GET /api/system/info
- GET /api/system/dependencies
- GET /api/watchlist-items
- POST /api/watchlist-items
- GET /api/watchlist-items/{symbol}
- POST /api/watchlist-items/{symbol}/refresh
- GET /api/watchlist-items/{symbol}/snapshots
- POST /api/watchlist-items/{symbol}/refresh-async
- GET /api/observability/summary

---

## Verification Summary

| Area | Status |
|---|---|
| RabbitMQ Docker setup | Completed |
| RabbitMQ configuration | Completed |
| PriceRefreshMessage contract | Completed |
| RabbitMQ publisher | Completed |
| Async refresh endpoint | Completed |
| Background Worker | Completed |
| Worker consumes queue messages | Verified |
| Worker reuses QuoteRefreshService | Verified |
| RabbitMQ publish log | Verified |
| Worker consume log | Verified |
| Async refresh creates PriceSnapshot | Verified |
| Structured logging | Completed |
| CorrelationId usage | Completed |
| Health endpoint | Verified |
| Dependency status endpoint | Verified |
| Observability summary endpoint | Verified |
| Swagger XML Summary visibility | Verified |
| Project build | Passed |

---

## Related Documentation

| Document | Purpose |
|---|---|
| `docs/messaging/rabbitmq-async-processing.md` | RabbitMQ async refresh publishing, queue behavior, message contract, Docker Compose setup and Swagger verification |
| `docs/messaging/background-worker-design.md` | Background Worker consume behavior, service scope usage, acknowledgement behavior and QuoteRefreshService reuse |
| `docs/observability/logging-standard.md` | Structured logging, cache logs, RabbitMQ logs and CorrelationId behavior |
| `docs/observability/basic-observability.md` | Health, dependency status and observability summary behavior |
| `docs/api-contracts/watchlist-quote-endpoints-api-contract.md` | Sync refresh, snapshot listing and watchlist quote endpoint behavior |
| `docs/final-demo-guide.md` | Final demo flow and verification order |
| `docs/final-technical-summary.md` | Final technical architecture and implementation summary |

---

## Demo Checklist

Before demo, verify:

- Docker Desktop is running.
- Redis container is running.
- RabbitMQ container is running.
- Project builds successfully.
- API starts successfully.
- Swagger opens successfully.
- GET /api/health returns 200 OK.
- GET /api/system/dependencies returns 200 OK.
- GET /api/watchlist-items returns 200 OK.
- Sync refresh returns 200 OK.
- Cache miss behavior is visible.
- Cache hit behavior is visible.
- Snapshot listing returns saved PriceSnapshot records.
- Async refresh returns 202 Accepted.
- RabbitMQ publish log is visible.
- Worker consume log is visible.
- Async refresh creates a new PriceSnapshot.
- GET /api/observability/summary returns 200 OK.
- Swagger XML Summary is visible.
- GitHub Project Board reflects completed work.

---

## Summary

Week 4 added asynchronous processing, structured logging and basic observability to the MarketInsight Operations Tracker API.

The implementation now supports:

- RabbitMQ-based async refresh publishing
- queued price refresh messages
- Background Worker message consumption
- reuse of existing QuoteRefreshService business flow
- CorrelationId-based async flow tracking
- structured logging for refresh, cache, RabbitMQ and worker processing
- dependency status checks
- observability summary endpoint
- Swagger-based final verification

The project is now ready for final MVP documentation, demo preparation and technical review.