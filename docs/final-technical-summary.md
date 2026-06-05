# Final Technical Summary

This document provides the final technical summary of the MarketInsight Operations Tracker API.

The goal of this document is to summarize the implemented backend architecture, main technical decisions, runtime flows, verification points, and demo-ready capabilities of the project.

---

## Project Summary

MarketInsight Operations Tracker API is a learning-focused ASP.NET Core Web API project.

The project was built to practice backend development concepts through a small financial tracking domain.

The API allows tracked financial symbols to be managed, refreshed from an external finance API, cached through Redis, persisted as PriceSnapshot records in SQLite, and processed asynchronously through RabbitMQ and a Background Worker.

The project focuses on learning and explaining:

- API design.
- Layered backend architecture.
- Entity and DTO separation.
- SQLite persistence with EF Core.
- Repository and Service layer responsibility.
- External API integration.
- User Secrets configuration.
- Redis cache-aside behavior.
- RabbitMQ async messaging.
- Background Worker processing.
- Structured logging and correlation.
- Basic observability endpoints.
- Swagger and XML Summary documentation.
- GitHub Issues and Project Board tracking.

This project is not a production-grade financial platform.

It is a backend learning project designed to demonstrate end-to-end system understanding.

---

## Repository Scope

This repository contains only the backend API component.

It represents:

- A .NET 8 ASP.NET Core Web API project.
- A learning-focused backend API.
- A technical practice project for API design, persistence, caching, messaging, logging, observability and documentation.

It does not represent:

- A frontend application.
- A mobile application.
- A commercial financial product.
- A trading platform.
- A financial advice system.
- A production-grade monitoring system.

---

## Implemented MVP Capabilities

The current MVP includes:

- Basic API health endpoint.
- Basic system information endpoint.
- Dependency status endpoint.
- Watchlist item listing.
- Watchlist item creation.
- Watchlist item lookup by symbol.
- Watchlist item soft delete behavior.
- Watchlist item reactivation behavior.
- External finance quote retrieval.
- User Secrets based API key configuration.
- Quote provider abstraction.
- Redis cache-aside behavior.
- Cache hit and cache miss logging.
- PriceSnapshot persistence in SQLite.
- PriceSnapshot listing by symbol.
- Synchronous quote refresh endpoint.
- Asynchronous quote refresh endpoint.
- RabbitMQ message publishing.
- RabbitMQ message consumption through Background Worker.
- CorrelationId usage in async flow.
- Structured logging for refresh, cache, messaging and worker processing.
- Basic observability summary endpoint.
- Swagger XML Summary documentation.
- Markdown technical documentation.

---

## Current Runtime API Surface

### Health and System Endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/health` | Returns basic API health information |
| GET | `/api/system/info` | Returns application name, version and environment |
| GET | `/api/system/dependencies` | Returns SQLite, Redis, RabbitMQ and external finance API status |
| GET | `/api/observability/summary` | Returns a summary of basic observability features |

---

### Watchlist Endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/watchlist-items` | Lists active watchlist items |
| GET | `/api/watchlist-items/{symbol}` | Gets an active watchlist item by symbol |
| POST | `/api/watchlist-items` | Creates or reactivates a watchlist item |
| DELETE | `/api/watchlist-items/{symbol}` | Soft deletes an active watchlist item |

---

### Quote and Snapshot Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/watchlist-items/{symbol}/refresh` | Refreshes quote data synchronously |
| GET | `/api/watchlist-items/{symbol}/snapshots` | Lists saved PriceSnapshot records for a symbol |
| POST | `/api/watchlist-items/{symbol}/refresh-async` | Queues an asynchronous quote refresh request |

---

## Architecture Style

The project follows a learning-focused layered monolith architecture.

This means:

- The project is a single backend API application.
- Responsibilities are separated into clear layers.
- The project does not use microservices.
- The goal is to keep the system understandable while still practicing professional backend concepts.

Main responsibility rule:

    Controller handles HTTP.
    Service handles business logic.
    Repository handles data access.
    Entity represents database persistence.
    DTO represents API contract.

---

## Layer Responsibilities

| Layer / Component | Responsibility |
|---|---|
| Controller | Receives HTTP requests and returns HTTP responses |
| Service | Coordinates use cases and business rules |
| Repository | Handles database access through EF Core |
| Entity | Represents database persistence model |
| DTO | Represents API request and response models |
| External API Client | Communicates with the external finance API |
| Quote Provider | Provides quote data to the service layer |
| Redis Cache Service | Handles short-term quote cache operations |
| RabbitMQ Publisher | Publishes async price refresh messages |
| Background Worker | Consumes queued messages and triggers quote refresh flow |
| AppDbContext | Represents EF Core database context |
| SQLite | Stores persistent application data |
| Swagger | Provides API testing and documentation surface |

---

## Tech Stack

| Area | Technology |
|---|---|
| Backend framework | ASP.NET Core Web API |
| Runtime | .NET 8 |
| Language | C# |
| API documentation | Swagger / OpenAPI |
| XML API documentation | XML Summary |
| Persistent database | SQLite |
| ORM | EF Core Code First |
| Cache | Redis |
| Messaging | RabbitMQ |
| Background processing | Background Worker |
| Configuration | appsettings + User Secrets |
| Documentation | Markdown |
| Tracking | GitHub Issues + GitHub Projects |

---

## Main Domain Concepts

The domain is based on financial symbol tracking.

| Domain Concept | Meaning |
|---|---|
| WatchlistItem | A financial symbol tracked by the system |
| PriceSnapshot | A saved quote result for a tracked symbol |
| PriceAlert | A future price condition concept |
| ActionItem | A future operational follow-up concept |

Current runtime implementation mainly focuses on:

- WatchlistItem.
- PriceSnapshot.
- Quote refresh flow.
- Cache behavior.
- Async refresh processing.

PriceAlert and ActionItem exist as domain concepts, but they should not be presented as completed runtime API flows unless implemented later.

---

## Data Persistence Summary

The project uses SQLite as the persistent database.

SQLite stores:

- WatchlistItem records.
- PriceSnapshot records.
- Other domain entities defined for the learning model.

EF Core Code First is used for:

- Entity configuration.
- DbContext management.
- Migration-based schema history.
- Database access through repositories.

Local database files should not be committed.

Migration files should be committed because they represent schema history.

---

## Watchlist Item Business Rules

Watchlist item behavior is based on normalized symbols.

Symbol normalization means:

    Trim whitespace.
    Convert to uppercase.

Example inputs:

    msft
    MSFT
    " msft "

All become:

    MSFT

Current behavior:

| Existing Record State | API Behavior | Status Code |
|---|---|---|
| No existing record | Create new WatchlistItem | 201 Created |
| Existing record is active | Return duplicate conflict | 409 Conflict |
| Existing record is inactive | Reactivate existing WatchlistItem | 200 OK |

The project uses soft delete.

Soft delete behavior:

    IsActive = false
    UpdatedAtUtc = current UTC time

Reactivation behavior:

    IsActive = true
    UpdatedAtUtc = current UTC time

The existing database row is reused.

No duplicate row is created for the same normalized symbol.

---

## Synchronous Quote Refresh Flow

The synchronous refresh endpoint is:

    POST /api/watchlist-items/{symbol}/refresh

The flow is:

    Swagger / Client
        ↓
    WatchlistItemsController
        ↓
    QuoteRefreshService
        ↓
    WatchlistItemRepository
        ↓
    RedisQuoteCacheService
        ↓
    QuoteProvider / FinanceQuoteClient if cache miss
        ↓
    RedisQuoteCacheService if quote is fetched externally
        ↓
    PriceSnapshotRepository
        ↓
    SQLite
        ↓
    QuoteRefreshResponse

This endpoint processes the refresh request during the HTTP request.

---

## Redis Cache-Aside Behavior

Redis is used only for short-term quote caching.

Current cache key format:

    quote:{SYMBOL}

Example:

    quote:MSFT

Current TTL:

    5 minutes

Cache miss flow:

    Refresh request
        ↓
    Redis lookup
        ↓
    Cache miss
        ↓
    External finance API call
        ↓
    Quote cached in Redis
        ↓
    PriceSnapshot saved in SQLite

Expected cache miss logs:

    Price refresh requested for symbol MSFT.
    Cache miss for symbol MSFT.
    Quote fetched from external API for symbol MSFT.
    Quote cached for symbol MSFT.
    Price snapshot saved for symbol MSFT.

Cache hit flow:

    Refresh request
        ↓
    Redis lookup
        ↓
    Cache hit
        ↓
    PriceSnapshot saved in SQLite

Expected cache hit logs:

    Price refresh requested for symbol MSFT.
    Cache hit for symbol MSFT.
    Price snapshot saved for symbol MSFT.

Important distinction:

    Redis stores short-term quote cache.
    SQLite stores persistent PriceSnapshot history.

---

## PriceSnapshot Persistence

Every successful quote refresh creates a PriceSnapshot record.

PriceSnapshot stores:

- WatchlistItemId.
- Symbol.
- Price.
- Currency.
- Source.
- RetrievedAtUtc.
- CreatedAtUtc.

Snapshot listing endpoint:

    GET /api/watchlist-items/{symbol}/snapshots

Purpose:

    Shows persisted historical quote refresh results for the selected symbol.

---

## External Finance API Integration

The project integrates with an external finance API through a separated client/provider structure.

Main responsibilities:

| Component | Responsibility |
|---|---|
| FinanceQuoteClient | Handles external HTTP communication |
| IQuoteProvider | Defines provider-facing quote retrieval contract |
| FinnhubQuoteProvider | Maps finance API data into internal quote response |
| FinanceApiOptions | Holds BaseUrl, ApiKey and Provider values |

API key handling:

    User Secrets are used during local development.
    API key is not hardcoded in source code.

This keeps external API configuration separate from source code.

---

## User Secrets Configuration

The external finance API key is configured with User Secrets.

Example command:

    dotnet user-secrets set "FinanceApi:ApiKey" "YOUR_FINANCE_API_KEY" --project src/MarketInsight.Api

Reason:

    API keys should not be committed to Git.
    Local development secrets should stay outside source code.

---

## Asynchronous Quote Refresh Flow

The async refresh endpoint is:

    POST /api/watchlist-items/{symbol}/refresh-async

Expected response:

    202 Accepted

Response shape:

    {
        "message": "Price refresh request accepted.",
        "symbol": "MSFT",
        "status": "Queued"
    }

The async flow is:

    Swagger / Client
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

The async endpoint queues the work.

The Background Worker consumes the message and reuses QuoteRefreshService.

The worker does not duplicate quote refresh business logic.

---

## RabbitMQ Messaging Summary

RabbitMQ is used for asynchronous refresh requests.

Main queue:

    price-refresh-queue

Message contract:

    PriceRefreshMessage

Message fields:

| Field | Purpose |
|---|---|
| Symbol | Symbol requested for refresh |
| RequestedAtUtc | UTC time when the async request was created |
| CorrelationId | Identifier used to connect publish and consume logs |

RabbitMQ publisher:

    RabbitMqPriceRefreshPublisher

Background consumer:

    PriceRefreshBackgroundWorker

Expected publish log:

    Async price refresh message published for symbol MSFT with correlation id ...

Expected consume log:

    Async price refresh message consumed for symbol MSFT with correlation id ...

Expected completion log:

    Queued price refresh completed for symbol MSFT

RabbitMQ Management UI:

    http://localhost:15672

Expected queue state after processing:

    Ready: 0
    Unacked: 0

---

## Background Worker Summary

The Background Worker runs with the API application.

Worker class:

    PriceRefreshBackgroundWorker

Main responsibilities:

- Connect to RabbitMQ.
- Consume messages from `price-refresh-queue`.
- Deserialize PriceRefreshMessage.
- Create log scope with CorrelationId and Symbol.
- Resolve IQuoteRefreshService through IServiceScopeFactory.
- Call QuoteRefreshService.RefreshQuoteAsync.
- Acknowledge successful messages.
- Log processing failures.

Important design decision:

    The worker coordinates async processing.
    QuoteRefreshService owns the business logic.

This keeps the system easier to understand and avoids duplicate refresh rules.

---

## Structured Logging Summary

The project uses structured logging placeholders.

Preferred format:

    _logger.LogInformation(
        "Cache hit for symbol {Symbol}.",
        normalizedSymbol);

Avoided format:

    _logger.LogInformation($"Cache hit for symbol {normalizedSymbol}.");

Reason:

    Structured logging keeps values such as Symbol, QueueName and CorrelationId trackable.

Main logged events:

- Price refresh requested.
- Cache miss.
- Cache hit.
- Quote fetched from external API.
- Quote cached.
- PriceSnapshot saved.
- RabbitMQ message published.
- RabbitMQ message consumed.
- Queued refresh completed.
- Dependency status checks.

---

## CorrelationId Summary

CorrelationId is used in the async RabbitMQ flow.

Current source:

    HttpContext.TraceIdentifier

The async endpoint adds CorrelationId to PriceRefreshMessage.

The publisher logs the CorrelationId.

The worker reads the CorrelationId and includes it in the log scope.

Purpose:

    It connects the HTTP request, RabbitMQ publish event and worker consume event.

This makes the async flow easier to follow from logs.

---

## Basic Observability Summary

The project includes basic observability endpoints.

| Endpoint | Purpose |
|---|---|
| GET /api/health | Shows whether the API is running |
| GET /api/system/info | Shows application name, version and environment |
| GET /api/system/dependencies | Shows SQLite, Redis, RabbitMQ and external API status |
| GET /api/observability/summary | Shows available observability features |

Dependency status values:

| Status | Meaning |
|---|---|
| Healthy | Dependency is available |
| Unhealthy | Dependency check failed |
| NotConfigured | Required configuration is missing |

External finance API behavior:

    If API key is missing, ExternalFinanceApi returns NotConfigured.
    If API key exists, the dependency check uses a short timeout.

Current timeout:

    3 seconds

---

## Swagger and XML Summary

Swagger is used as the main local testing surface.

XML Summary comments are used to improve Swagger documentation.

Important Swagger-visible endpoints:

- GET /api/health.
- GET /api/system/info.
- GET /api/system/dependencies.
- GET /api/watchlist-items.
- POST /api/watchlist-items.
- GET /api/watchlist-items/{symbol}.
- DELETE /api/watchlist-items/{symbol}.
- POST /api/watchlist-items/{symbol}/refresh.
- GET /api/watchlist-items/{symbol}/snapshots.
- POST /api/watchlist-items/{symbol}/refresh-async.
- GET /api/observability/summary.

Purpose:

    Swagger proves that the API is testable and documented.

---

## Docker Compose Summary

Docker Compose is used to start local dependencies.

Current local dependencies:

- Redis.
- RabbitMQ.

Command:

    docker compose up -d

Verification command:

    docker ps

Expected containers:

    marketinsight-redis
    marketinsight-rabbitmq

Redis test:

    docker exec -it marketinsight-redis redis-cli ping

Expected response:

    PONG

RabbitMQ Management UI:

    http://localhost:15672

---

## Final Verification Results

The final verification flow confirms:

- Project builds successfully.
- Docker Compose starts Redis.
- Docker Compose starts RabbitMQ.
- Health endpoint returns 200 OK.
- System info endpoint returns 200 OK.
- Dependency status endpoint returns 200 OK.
- Watchlist item listing works.
- Watchlist item creation works.
- Watchlist item lookup by symbol works.
- Synchronous quote refresh works.
- Cache miss behavior is visible.
- Cache hit behavior is visible.
- PriceSnapshot listing works.
- Async refresh endpoint returns 202 Accepted.
- RabbitMQ publish log is visible.
- Worker consume log is visible.
- Async refresh creates a new PriceSnapshot.
- Observability summary endpoint returns 200 OK.
- Swagger XML Summary documentation is visible.

---

## Documentation Structure Summary

Main documentation entry point:

    docs/00-index.md

Important documentation areas:

| Area | Purpose |
|---|---|
| architecture | Architecture and responsibility standards |
| api-contracts | API endpoint contracts and Swagger behavior |
| database-design | Entity, EF Core, SQLite and constraint standards |
| integrations | External finance API integration |
| configuration | User Secrets and provider abstraction |
| cache | Redis cache design |
| messaging | RabbitMQ and Background Worker design |
| observability | Logging and dependency status documentation |
| project-tracking | Weekly progress summaries |

Final preparation documents:

    docs/final-demo-guide.md
    docs/final-technical-summary.md

---

## GitHub Tracking Summary

The project uses GitHub Issues and GitHub Projects to track implementation work.

Tracking approach:

- Work is split into small issues.
- Each issue has a clear scope.
- Acceptance criteria are used to verify completion.
- Implementation, testing and documentation are tracked together.
- Project Board is used to visualize progress.

This supports professional project discipline, not only coding.

---

## Out of Scope

The current MVP does not include:

- Authentication.
- User-specific watchlists.
- Multiple portfolios.
- Trading operations.
- Financial advice.
- Real-time streaming market data.
- Advanced investment analytics.
- Advanced monitoring dashboard.
- OpenTelemetry.
- Alert evaluation endpoint.
- ActionItem creation flow.
- Frontend application.
- Mobile application.
- Multi-tenant production architecture.
- Production-grade security hardening.

---

## Main Technical Decisions

| Area | Decision |
|---|---|
| Architecture | Learning-focused layered monolith |
| Database | SQLite for persistent local data |
| ORM | EF Core Code First |
| Cache | Redis for short-term quote cache only |
| Messaging | RabbitMQ for async refresh queue only |
| Worker | Background Worker consumes queued refresh messages |
| Business logic | QuoteRefreshService owns refresh rules |
| API contract | DTOs are returned instead of Entities |
| Configuration | User Secrets are used for external API key |
| Documentation | Markdown docs and XML Summary |
| Testing surface | Swagger |
| Tracking | GitHub Issues and GitHub Projects |

---

## Key Learning Outcomes

This project demonstrates practice with:

- ASP.NET Core Web API project structure.
- HTTP and REST endpoint design.
- Controller, Service and Repository separation.
- Entity and DTO separation.
- EF Core and SQLite persistence.
- Migration-based database development.
- Public finance API integration.
- User Secrets configuration.
- Redis cache-aside behavior.
- Cache hit and cache miss verification.
- RabbitMQ message publishing.
- Background Worker message consumption.
- Structured logging.
- CorrelationId usage in async processing.
- Basic dependency observability.
- Swagger endpoint testing.
- Markdown technical documentation.
- GitHub-based project tracking.

---

## Review Questions

1. What problem does this API solve in the learning domain?
2. Why is the project designed as a layered monolith?
3. What is the responsibility of the Controller layer?
4. What is the responsibility of the Service layer?
5. What is the responsibility of the Repository layer?
6. Why are DTOs used instead of returning Entities directly?
7. Why is SQLite used in this project?
8. Why is Redis used only as a cache?
9. What is the difference between cache hit and cache miss?
10. Why is PriceSnapshot stored in SQLite?
11. Why is RabbitMQ used in the async refresh flow?
12. What does the Background Worker do?
13. Why does the worker reuse QuoteRefreshService?
14. What is CorrelationId used for?
15. Which endpoints prove basic observability?
16. What is intentionally out of scope for the MVP?

---

## Summary

MarketInsight Operations Tracker API is a complete learning-focused backend MVP.

The project demonstrates a realistic backend flow:

    Watchlist item
        ↓
    Quote refresh
        ↓
    Redis cache
        ↓
    External finance API if needed
        ↓
    PriceSnapshot persistence
        ↓
    Async RabbitMQ processing
        ↓
    Background Worker
        ↓
    Logging and observability
        ↓
    Swagger verification
        ↓
    Technical documentation

The main value of the project is not only that endpoints work.

The main value is that the system can be explained clearly:

    What each component does.
    Why each component exists.
    How the data flows through the system.
    How the implementation was verified.
    How the work was documented and tracked.

This makes the project ready for final technical demo and review.