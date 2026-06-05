# Final Demo Guide

This document provides the final demo guide for the MarketInsight Operations Tracker API.

The goal of this guide is to help present the project clearly, verify the main MVP flows through Swagger, and explain the technical decisions behind the implementation.

---

## Demo Goal

The final demo should show that the MarketInsight Operations Tracker API is a working learning-focused backend project.

The demo should prove that the API can:

- Manage watchlist items.
- Retrieve quote data from an external finance API.
- Use Redis cache-aside behavior.
- Store PriceSnapshot records in SQLite.
- Publish async refresh messages to RabbitMQ.
- Consume queued messages with a Background Worker.
- Show structured logs and CorrelationId-based async flow.
- Expose basic health, dependency and observability endpoints.
- Provide Swagger-testable API documentation.
- Maintain technical documentation under the `docs/` folder.

The purpose of the demo is not to show a production-grade financial system.

The purpose is to explain how the backend flow works, why each component exists, and how the project was built step by step.

---


## Explanation Focus

- Problem definition.
- Backend architecture.
- Request-response flow.
- Data persistence.
- Cache behavior.
- Async processing.
- Observability.
- Documentation and project tracking.

---

## Recommended Demo Duration

Recommended technical demo duration:

    15 minutes

Suggested structure:

| Section | Duration |
|---|---|
| Project introduction | 1 minute |
| Architecture and tech stack | 3 minutes |
| Swagger endpoint demo | 7 minutes |
| Redis, RabbitMQ, Worker and logging explanation | 3 minutes |
| Documentation and project tracking summary | 1 minute |

The remaining meeting time can be used for questions and discussion.

---

## Demo Preparation Checklist

Before starting the demo, verify the following:

- The project builds successfully.
- Docker Desktop is running.
- Redis container is running.
- RabbitMQ container is running.
- API application starts successfully.
- Swagger UI opens correctly.
- External finance API key is configured if external API tests will be shown.
- RabbitMQ Management UI is reachable.
- GitHub repository is available.
- GitHub Issues and Project Board are updated.
- Final documentation files are committed.

Recommended commands:

    dotnet build
    docker compose up -d
    docker ps

Expected containers:

    marketinsight-redis
    marketinsight-rabbitmq

---

## Local Run Commands

Restore dependencies:

    dotnet restore

Build the solution:

    dotnet build

Start Docker dependencies:

    docker compose up -d

Run the API project:

    dotnet run --project src/MarketInsight.Api/MarketInsight.Api.csproj

Open Swagger using the local URL shown in the terminal.

---

## RabbitMQ Management UI

RabbitMQ Management UI can be opened from:

    http://localhost:15672

Default local credentials:

    username: guest
    password: guest

Main queue:

    price-refresh-queue

During async refresh testing, the queue should return to:

    Ready: 0
    Unacked: 0

This shows that the worker consumed the queued message.

---

## Demo Script

The demo can be explained with the following opening:

    MarketInsight Operations Tracker API is a learning-focused ASP.NET Core Web API project.
    The goal is to practice backend development concepts through a small financial tracking domain.
    The project manages tracked financial symbols, retrieves quote data, uses Redis cache, stores price snapshots in SQLite, and processes async refresh messages with RabbitMQ and a Background Worker.

Then continue with:

    The project is not a production trading platform.
    It is a backend learning project focused on API design, persistence, caching, messaging, logging, observability, documentation and GitHub-based tracking.

---

## Architecture Explanation

The project follows a learning-focused layered monolith architecture.

Main layers:

| Layer | Responsibility |
|---|---|
| Controller | Handles HTTP requests and responses |
| Service | Handles business use cases and business rules |
| Repository | Handles database access |
| Entity | Represents database persistence model |
| DTO | Represents API request and response contract |
| External API Client | Communicates with the finance API |
| Redis Cache Service | Stores short-term quote data |
| RabbitMQ Publisher | Publishes async refresh messages |
| Background Worker | Consumes queued refresh messages |

Core explanation:

    Controller handles HTTP.
    Service handles business logic.
    Repository handles data access.
    Entity represents persistence.
    DTO represents API contract.

---

## Tech Stack Explanation

| Area | Technology |
|---|---|
| Backend framework | ASP.NET Core Web API |
| Runtime | .NET 8 |
| Language | C# |
| API documentation | Swagger / OpenAPI |
| Persistent database | SQLite |
| ORM | EF Core Code First |
| Cache | Redis |
| Messaging | RabbitMQ |
| Background processing | Background Worker |
| Configuration | User Secrets |
| Documentation | Markdown + XML Summary |
| Tracking | GitHub Issues + GitHub Projects |

Short explanation:

    SQLite is used for persistent data.
    Redis is used only for short-term quote caching.
    RabbitMQ is used only for asynchronous refresh processing.
    Swagger is used to test and document endpoints.
    Markdown documentation explains technical decisions.

---

## Main Demo Flow

The recommended Swagger demo order is:

1. Health and system endpoints.
2. Watchlist CRUD flow.
3. Synchronous quote refresh.
4. Cache miss behavior.
5. Cache hit behavior.
6. PriceSnapshot listing.
7. Async refresh through RabbitMQ.
8. Background Worker consume flow.
9. Observability summary.
10. Documentation and GitHub tracking.

---

## Health and System Demo

### Health Endpoint

Request:

    GET /api/health

Expected result:

    200 OK

Expected response includes:

    status
    application
    timestamp

Purpose:

    Shows that the API application is running.

---

### System Info Endpoint

Request:

    GET /api/system/info

Expected result:

    200 OK

Expected response includes:

    applicationName
    version
    environment

Purpose:

    Shows basic application and environment information.

---

### Dependency Status Endpoint

Request:

    GET /api/system/dependencies

Expected result:

    200 OK

Expected dependencies:

- SQLite.
- Redis.
- RabbitMQ.
- ExternalFinanceApi.

Expected status values:

- Healthy.
- Unhealthy.
- NotConfigured.

Purpose:

    Shows whether the main application dependencies are reachable.

Important note:

    If the external finance API key is missing, ExternalFinanceApi should return NotConfigured instead of a generic failure.

---

## Watchlist CRUD Demo

### List Watchlist Items

Request:

    GET /api/watchlist-items

Expected result:

    200 OK

Purpose:

    Shows active tracked financial symbols.

---

### Create Watchlist Item

Request:

    POST /api/watchlist-items

Example request body:

    {
        "symbol": "MSFT",
        "displayName": "Microsoft Corporation",
        "market": "US"
    }

Expected possible results:

| Status Code | Meaning |
|---|---|
| 201 Created | New watchlist item was created |
| 200 OK | Existing inactive symbol was reactivated |
| 409 Conflict | Active duplicate already exists |

Purpose:

    Shows create, duplicate and reactivation behavior.

---

### Get Watchlist Item By Symbol

Request:

    GET /api/watchlist-items/MSFT

Expected result:

    200 OK

Purpose:

    Shows symbol-based lookup behavior.

---

## Watchlist Item Business Rule Explanation

Watchlist items use normalized symbols.

Example:

    msft
    MSFT
    " msft "

All represent:

    MSFT

The project uses `NormalizedSymbol` to prevent duplicate active records.

Current behavior:

| Existing Record State | API Behavior | Status Code |
|---|---|---|
| No existing record | Create new WatchlistItem | 201 Created |
| Existing record is active | Return duplicate conflict | 409 Conflict |
| Existing record is inactive | Reactivate existing WatchlistItem | 200 OK |

The project uses soft delete.

When a symbol is deleted:

    IsActive = false

When the same symbol is posted again:

    IsActive = true

The existing row is reused.

---

## Synchronous Quote Refresh Demo

### Cache Miss Preparation

To demonstrate cache miss behavior, clear the Redis key before refresh:

    docker exec -it marketinsight-redis redis-cli DEL quote:MSFT

---

### Refresh Quote

Request:

    POST /api/watchlist-items/MSFT/refresh

Expected result:

    200 OK

Expected cache miss logs:

    Price refresh requested for symbol MSFT.
    Cache miss for symbol MSFT.
    Quote fetched from external API for symbol MSFT.
    Quote cached for symbol MSFT.
    Price snapshot saved for symbol MSFT.

Purpose:

    Shows external API integration, Redis cache write and SQLite snapshot persistence.

---

## Cache Hit Demo

Run the same refresh request again:

    POST /api/watchlist-items/MSFT/refresh

Expected result:

    200 OK

Expected cache hit logs:

    Price refresh requested for symbol MSFT.
    Cache hit for symbol MSFT.
    Price snapshot saved for symbol MSFT.

Purpose:

    Shows that Redis is used as a short-term cache and avoids unnecessary external API calls.

---

## PriceSnapshot Listing Demo

Request:

    GET /api/watchlist-items/MSFT/snapshots

Expected result:

    200 OK

Expected response:

    A list of saved PriceSnapshot records for the selected symbol.

Purpose:

    Shows that quote refresh results are persisted in SQLite.

Important explanation:

    Redis stores short-term quote data.
    SQLite stores persistent PriceSnapshot history.

---

## Async Refresh Demo

### Async Refresh Endpoint

Request:

    POST /api/watchlist-items/MSFT/refresh-async

Expected result:

    202 Accepted

Expected response:

    {
        "message": "Price refresh request accepted.",
        "symbol": "MSFT",
        "status": "Queued"
    }

Purpose:

    Shows that the API accepts the request and queues the work instead of processing everything inside the HTTP request.

---

## RabbitMQ Publish Log

Expected publish log:

    Async price refresh message published for symbol MSFT with correlation id ...

Purpose:

    Shows that the async refresh request created a RabbitMQ message.

---

## Background Worker Consume Log

Expected consume log:

    Async price refresh message consumed for symbol MSFT with correlation id ...

Expected completion log:

    Queued price refresh completed for symbol MSFT

Purpose:

    Shows that the Background Worker consumed the message and reused the existing quote refresh flow.

---

## RabbitMQ Queue Verification

Open RabbitMQ Management UI:

    http://localhost:15672

Check queue:

    price-refresh-queue

Expected after processing:

    Ready: 0
    Unacked: 0

Purpose:

    Shows that the queued message was consumed successfully.

---

## Async Snapshot Verification

After async refresh, call:

    GET /api/watchlist-items/MSFT/snapshots

Expected result:

    A new PriceSnapshot appears with a recent CreatedAtUtc value.

Purpose:

    Shows that async refresh also creates persistent snapshot records.

---

## Async Flow Explanation

The async refresh flow is:

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

Important explanation:

    The worker does not duplicate quote refresh business logic.
    It consumes the message and calls QuoteRefreshService.
    This keeps business rules in one place.

---

## Logging and Correlation Demo

The async flow carries a CorrelationId.

Current source:

    HttpContext.TraceIdentifier

The message contains:

    Symbol
    RequestedAtUtc
    CorrelationId

The publisher logs the CorrelationId.

The worker reads the CorrelationId and uses it in the log scope.

Purpose:

    This helps connect the publish and consume parts of the async flow.

---

## Observability Summary Demo

Request:

    GET /api/observability/summary

Expected result:

    200 OK

Expected response includes:

    applicationName
    environment
    generatedAtUtc
    summary
    endpoints
    observabilityFeatures

Purpose:

    Shows a beginner-friendly summary of available observability features.

---

## Swagger XML Summary Demo

In Swagger, verify that endpoint descriptions are visible.

Important endpoints:

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

Purpose:

    Shows that XML Summary documentation is connected to Swagger.

---

## Documentation Demo

Show the `docs/` folder.

Important documentation areas:

| Area | Purpose |
|---|---|
| architecture | Architecture and responsibility standards |
| api-contracts | Endpoint and API behavior contracts |
| database-design | Entity, EF Core and SQLite design |
| integrations | External finance API integration |
| configuration | User Secrets and provider abstraction |
| cache | Redis cache design |
| messaging | RabbitMQ and Background Worker design |
| observability | Logging and dependency status documentation |
| project-tracking | Weekly progress summaries |

Important final documents:

    docs/final-demo-guide.md
    docs/final-technical-summary.md

Purpose:

    Shows that the implementation is supported by technical documentation.

---

## GitHub Tracking Demo

Show GitHub Issues and GitHub Project Board.

Explain:

    Work was split into small issues.
    Each issue had scope and acceptance criteria.
    Implementation, testing and documentation were tracked together.
    GitHub Project Board was used to follow progress.

Purpose:

    Shows project tracking discipline, not only coding.

---

## What Should Not Be Presented As Implemented

The current demo should not present the following as completed runtime features unless they are implemented later:

- Authentication.
- User-specific watchlists.
- Trading operations.
- Financial advice.
- Real-time streaming market data.
- Advanced monitoring dashboard.
- OpenTelemetry.
- Alert evaluation endpoint.
- ActionItem creation flow.

PriceAlert and ActionItem may be mentioned as domain concepts, but they should not be presented as completed API flows if they are not implemented.

---

## Common Questions and Suggested Answers

### Why SQLite?

SQLite is simple for local development and learning. It allows EF Core Code First, migrations and persistence practice without requiring a separate database server.

### Why Redis?

Redis is used for short-term quote caching. It helps demonstrate cache-aside behavior, cache hit and cache miss logs, and reduced external API calls.

### Why RabbitMQ?

RabbitMQ is used to demonstrate asynchronous processing. The API can accept a refresh request and queue work for the Background Worker.

### Why Background Worker?

The Background Worker consumes queued messages from RabbitMQ and triggers the existing quote refresh flow. This keeps async processing inside the backend project without creating a separate worker service.

### Why not microservices?

The project is learning-focused. A layered monolith is simpler, easier to reason about and enough for the MVP scope.

### Why not production monitoring?

The project focuses on basic observability first. Advanced monitoring tools like OpenTelemetry, dashboards and centralized log storage are outside the current MVP scope.

### Why use DTOs?

DTOs separate API contracts from database entities. This prevents the API from exposing persistence models directly.

### Why use Repository Pattern?

Repository Pattern keeps database access separate from business logic and makes the Service layer easier to understand.

### Why use User Secrets?

User Secrets keep the external finance API key out of source code during local development.

---

## Final Demo Closing Statement

A possible closing statement:

    This project helped me practice backend API development as an end-to-end workflow.
    I worked on API design, SQLite persistence, EF Core, Repository and Service layers, external API integration, Redis cache, RabbitMQ async processing, Background Worker, structured logging, basic observability, Swagger testing and technical documentation.
    My main focus was not only writing code, but understanding why each component exists and how the full system flow works.

---

## Final Verification Summary

Before completing the demo, confirm:

- Project builds successfully.
- Redis starts through Docker Compose.
- RabbitMQ starts through Docker Compose.
- Health endpoint works.
- Dependency status endpoint works.
- Watchlist CRUD flow works.
- Sync refresh flow works.
- Cache miss behavior is visible.
- Cache hit behavior is visible.
- Snapshot listing works.
- Async refresh returns 202 Accepted.
- RabbitMQ publish log is visible.
- Worker consume log is visible.
- Async refresh creates PriceSnapshot.
- Observability summary works.
- Swagger XML Summary is visible.
- Documentation folder is complete.
- GitHub Issues and Project Board are updated.

---

## Summary

The final demo should prove that the MarketInsight Operations Tracker API is a working learning-focused backend API.

The strongest demo path is:

    Health
        ↓
    Dependencies
        ↓
    Watchlist CRUD
        ↓
    Sync refresh
        ↓
    Redis cache miss / hit
        ↓
    PriceSnapshot persistence
        ↓
    Async refresh
        ↓
    RabbitMQ publish / consume
        ↓
    Background Worker processing
        ↓
    Observability summary
        ↓
    Documentation and GitHub tracking

This flow shows both implementation and understanding.

The key message of the demo is:

    I can explain what each component does, why it exists, and how the full backend flow works.