# MarketInsight Operations Tracker API

MarketInsight Operations Tracker API is a learning-focused backend API project built with ASP.NET Core Web API.

The project is designed to practice professional backend development concepts through a small financial tracking domain.

This repository contains only the backend API component of the MarketInsight Operations Tracker learning project.

---

## Repository Scope

This repository represents:

- A learning-focused backend API
- A .NET 8 ASP.NET Core Web API project
- A backend practice project for API design, persistence, caching, messaging, logging, observability, and documentation

This repository does not represent:

- A frontend application
- A mobile application
- A complete commercial product
- A production-grade financial trading platform
- A real trading or portfolio management system
- A system that provides financial advice

---

## Domain Definition

MarketInsight Operations Tracker is a learning-focused backend API that tracks financial symbols, retrieves market quote data, stores price snapshots, and prepares a foundation for future operational alert and action workflows.

The main domain concepts are:

| Domain Concept | Meaning |
|---|---|
| `WatchlistItem` | A financial symbol tracked by the system |
| `PriceSnapshot` | A saved price record for a tracked symbol |
| `PriceAlert` | A future price condition concept for a tracked symbol |
| `ActionItem` | A future operational follow-up concept after an alert or important market event |

The main implemented runtime flow currently focuses on:

- WatchlistItem management
- Quote refresh
- PriceSnapshot persistence
- Redis cache behavior
- RabbitMQ async refresh processing
- Basic observability

The main entity is:

    WatchlistItem

All price tracking and refresh behavior starts from a tracked financial symbol.

PriceAlert and ActionItem are part of the broader domain model, but they are not presented as completed runtime API flows in the current MVP.

---

## Project Goal

The goal of this project is not to build a production-grade financial product.

The goal is to learn how a backend API is structured, developed, tested, documented, and tracked in a professional workflow.

The project focuses on practicing:

- ASP.NET Core Web API fundamentals
- HTTP and REST API design
- Controller-based API development
- Swagger / OpenAPI documentation
- XML Summary documentation
- Entity and DTO separation
- SQLite and EF Core persistence
- Repository Pattern and LINQ queries
- Public API integration
- User Secrets configuration
- Redis cache-aside behavior
- RabbitMQ asynchronous messaging
- Background Worker processing
- Structured logging and correlation
- Basic observability endpoints
- GitHub Issues and GitHub Projects tracking
- Technical documentation discipline

---

## Architecture Style

The project follows a learning-focused layered monolith architecture.

This means:

- The application is developed as a single backend API.
- The code is separated into clear responsibility layers.
- The project does not use a microservice architecture.
- The main goal is to learn backend responsibility separation step by step.

Main layers and components:

| Layer / Component | Responsibility |
|---|---|
| Controller | Handles HTTP request and response flow |
| Service | Handles use cases and business rules |
| Repository | Handles database access |
| Entity | Represents database persistence model |
| DTO | Represents API request and response contract |
| External API Client | Communicates with the external finance API |
| Redis Cache Service | Handles short-term quote cache operations |
| RabbitMQ Publisher | Publishes async price refresh messages |
| Background Worker | Consumes queued refresh messages |

Core rule:

    Controller handles HTTP.
    Service handles business logic.
    Repository handles data access.
    Entity represents database persistence.
    DTO represents API contract.

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
| Documentation | Markdown + XML Summary |
| Tracking | GitHub Issues + GitHub Projects |

---

## Implemented MVP Capabilities

The current MVP includes:

- Manage financial symbol watchlist items
- List active watchlist items
- Create new watchlist items
- Reject active duplicate symbols
- Reactivate inactive watchlist items
- Soft delete watchlist items
- Retrieve quote data from a public finance API
- Configure the external finance API key with User Secrets
- Cache quote data in Redis
- Log cache hit and cache miss behavior
- Persist PriceSnapshot records in SQLite
- List saved PriceSnapshot records by symbol
- Refresh quote data synchronously
- Send asynchronous price refresh messages to RabbitMQ
- Process queued refresh messages with a Background Worker
- Reuse QuoteRefreshService in both sync and async refresh flows
- Use structured logging placeholders
- Carry CorrelationId through the async RabbitMQ flow
- Provide health, system, dependency and observability endpoints
- Display Swagger / OpenAPI documentation
- Use XML Summary comments for API documentation
- Track work with GitHub Issues and GitHub Projects
- Maintain Markdown technical documentation under the `docs/` folder

---

## Current Runtime API Surface

### Health, System and Observability Endpoints

| Operation | HTTP Method | Route |
|---|---|---|
| Check API health | GET | `/api/health` |
| Get system information | GET | `/api/system/info` |
| Check dependency status | GET | `/api/system/dependencies` |
| Get observability summary | GET | `/api/observability/summary` |

### Watchlist Endpoints

| Operation | HTTP Method | Route |
|---|---|---|
| List watchlist items | GET | `/api/watchlist-items` |
| Get watchlist item by symbol | GET | `/api/watchlist-items/{symbol}` |
| Create or reactivate watchlist item | POST | `/api/watchlist-items` |
| Delete or deactivate watchlist item | DELETE | `/api/watchlist-items/{symbol}` |

### Quote and Snapshot Endpoints

| Operation | HTTP Method | Route |
|---|---|---|
| Refresh quote data for a symbol synchronously | POST | `/api/watchlist-items/{symbol}/refresh` |
| List saved price snapshots for a symbol | GET | `/api/watchlist-items/{symbol}/snapshots` |
| Queue quote refresh for a symbol asynchronously | POST | `/api/watchlist-items/{symbol}/refresh-async` |

---

## API Route Direction

The project uses resource-based API route naming.

For Watchlist CRUD, the selected route standard is:

    /api/watchlist-items

Reason:

- The MVP manages watchlist item resources.
- The system does not currently manage multiple watchlists.
- The route maps clearly to the `WatchlistItem` domain concept.
- The route is clearer than `/api/watchlist`.

Quote refresh and snapshot routes also follow the same watchlist route standard.

---

## Watchlist Item Behavior

The Watchlist Items API uses `NormalizedSymbol` to decide whether a symbol should be created, rejected as an active duplicate, or reactivated.

Current behavior:

| Existing Record State | API Behavior | Status Code |
|---|---|---|
| No existing record with same `NormalizedSymbol` | Create new `WatchlistItem` | `201 Created` |
| Existing record is active | Return duplicate conflict | `409 Conflict` |
| Existing record is inactive | Reactivate existing `WatchlistItem` | `200 OK` |

The project uses soft delete instead of hard delete.

When a watchlist item is deleted:

    IsActive = false
    UpdatedAtUtc = current UTC time

When the same inactive symbol is posted again:

    IsActive = true
    UpdatedAtUtc = current UTC time

The existing database row is reused.

No duplicate row is created for the same normalized symbol.

---

## Quote Refresh Behavior

The synchronous quote refresh endpoint is:

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

## Redis Cache Behavior

Redis is used for short-term quote caching.

Current cache key format:

    quote:{SYMBOL}

Example:

    quote:MSFT

Current TTL:

    5 minutes

Cache miss behavior:

    Redis does not contain quote data.
    The external finance API is called.
    The quote is cached in Redis.
    A PriceSnapshot is saved in SQLite.

Cache hit behavior:

    Redis already contains quote data.
    The external finance API is skipped.
    A PriceSnapshot is still saved in SQLite.

Important distinction:

    Redis stores short-term quote cache.
    SQLite stores persistent PriceSnapshot history.

---

## Async Refresh Behavior

The async quote refresh endpoint is:

    POST /api/watchlist-items/{symbol}/refresh-async

Expected response:

    202 Accepted

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

## Messaging Behavior

RabbitMQ is used only for asynchronous refresh processing.

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

Expected queue behavior:

    Message is published.
    Worker consumes the message.
    Worker calls QuoteRefreshService.
    Message is acknowledged after processing.

RabbitMQ Management UI:

    http://localhost:15672

Default local credentials:

    username: guest
    password: guest

---

## Logging and Correlation Behavior

The project uses structured logging placeholders.

Preferred format:

    _logger.LogInformation(
        "Cache hit for symbol {Symbol}.",
        normalizedSymbol);

Avoided format:

    _logger.LogInformation($"Cache hit for symbol {normalizedSymbol}.");

Main logged events:

- Price refresh requested
- Cache miss
- Cache hit
- Quote fetched from external API
- Quote cached
- PriceSnapshot saved
- RabbitMQ message published
- RabbitMQ message consumed
- Queued refresh completed
- Dependency status checks

CorrelationId is used in the async RabbitMQ flow.

Current source:

    HttpContext.TraceIdentifier

The CorrelationId connects:

    HTTP request
        ↓
    RabbitMQ publish
        ↓
    RabbitMQ consume
        ↓
    Background Worker processing

---

## Basic Observability Behavior

The project includes basic observability endpoints.

| Endpoint | Purpose |
|---|---|
| `GET /api/health` | Shows whether the API is running |
| `GET /api/system/info` | Shows application name, version and environment |
| `GET /api/system/dependencies` | Shows SQLite, Redis, RabbitMQ and external finance API status |
| `GET /api/observability/summary` | Shows available observability features |

Dependency status values:

| Status | Meaning |
|---|---|
| Healthy | Dependency is available |
| Unhealthy | Dependency check failed |
| NotConfigured | Required configuration is missing |

External finance API behavior:

    If the API key is missing, ExternalFinanceApi returns NotConfigured.
    If the API key exists, the dependency check uses a short timeout.

Current timeout:

    3 seconds

---

## Data Responsibility Standards

| Topic | Standard |
|---|---|
| Entity model | Database / persistence model |
| DTO model | API request / response contract |
| Entity exposure | Entity models should not be returned directly from API endpoints |
| Symbol uniqueness | `NormalizedSymbol` should be unique across all records |
| Symbol normalization | Trim whitespace and convert to uppercase |
| Soft delete | Use `IsActive = false` instead of hard delete |
| Reactivation | Re-posting an inactive symbol reactivates the existing row |
| Financial values | Use `decimal` |
| Date/time values | Use UTC and `Utc` suffix |
| SQLite database file | Do not commit local `.db` files |
| Migration files | Commit migration files |

---

## Documentation

Detailed technical documentation is organized under the `docs/` folder.

Main documentation entry point:

    docs/00-index.md

Documentation areas:

| Area | Location |
|---|---|
| Project overview | `docs/01-project-overview.md` |
| Final demo guide | `docs/final-demo-guide.md` |
| Final technical summary | `docs/final-technical-summary.md` |
| Architecture standards | `docs/architecture` |
| API contracts | `docs/api-contracts` |
| Database design | `docs/database-design` |
| External integrations | `docs/integrations` |
| Configuration | `docs/configuration` |
| Cache design | `docs/cache` |
| Messaging design | `docs/messaging` |
| Observability design | `docs/observability` |
| Project tracking | `docs/project-tracking` |

Important documentation files:

| Document | Purpose |
|---|---|
| `docs/00-index.md` | Main documentation index |
| `docs/01-project-overview.md` | High-level project overview |
| `docs/final-demo-guide.md` | Final demo flow, Swagger verification order, Redis/RabbitMQ checks, async refresh verification, logging checks, documentation demo, and common technical questions |
| `docs/final-technical-summary.md` | Implemented MVP architecture, technical decisions, runtime flows, verification results, and learning outcomes |
| `docs/architecture/project-naming-standard.md` | Project, repository, solution, namespace, folder, route, and documentation naming standards |
| `docs/architecture/layer-responsibility-standard.md` | Controller, Service, Repository, Entity, and DTO responsibility standards, including create, duplicate, soft delete, and reactivation decisions |
| `docs/architecture/api-route-naming-standard.md` | API route naming rules |
| `docs/architecture/repository-pattern-and-linq.md` | Repository Pattern, LINQ usage, async EF Core queries, data access separation, active/inactive record lookup, and repository support for soft delete and reactivation flows |
| `docs/architecture/service-layer-and-quote-refresh-flow.md` | Service-layer boundary and quote refresh flow design |
| `docs/database-design/entity-design.md` | Entity and DTO model design, including WatchlistItem lifecycle, soft delete, and reactivation behavior |
| `docs/database-design/entity-relationship-model.md` | Entity relationship model |
| `docs/database-design/entity-constraint-standards.md` | Symbol uniqueness, normalization, soft delete, reactivation, decimal, UTC, and persistence standards |
| `docs/database-design/ef-core-sqlite-setup.md` | EF Core and SQLite setup notes |
| `docs/api-contracts/xml-summary-swagger-standard.md` | XML Summary and Swagger documentation standard |
| `docs/api-contracts/api-endpoint-draft.md` | Initial controller endpoint draft and `GET /api/system/info` endpoint documentation |
| `docs/api-contracts/watchlist-items-api-contract.md` | Watchlist Items API contract, status codes, soft delete, reactivation behavior, and Swagger test flow |
| `docs/api-contracts/quote-refresh-api-contract.md` | Quote refresh service contract, cache-aside flow, controlled result behavior, and PriceSnapshot persistence rules |
| `docs/api-contracts/watchlist-quote-endpoints-api-contract.md` | Watchlist quote refresh and snapshot listing endpoint contract |
| `docs/integrations/public-finance-api-integration.md` | External finance API client flow, external response model, and internal quote DTO structure |
| `docs/configuration/user-secrets-and-strategy-pattern.md` | User Secrets setup and quote provider abstraction |
| `docs/cache/redis-cache-design.md` | Redis cache-aside behavior, `quote:{symbol}` key format, 5-minute TTL, and Docker Compose Redis setup |
| `docs/messaging/rabbitmq-async-processing.md` | RabbitMQ async refresh publishing, queue behavior, `PriceRefreshMessage`, Docker Compose RabbitMQ setup, and Swagger verification |
| `docs/messaging/background-worker-design.md` | Background Worker message consumption, RabbitMQ consumer behavior, service scope usage, acknowledgement behavior, and reuse of QuoteRefreshService |
| `docs/observability/logging-standard.md` | Structured logging rules, cache hit and miss logs, quote refresh logs, RabbitMQ publish and consume logs, CorrelationId usage, and runtime verification |
| `docs/observability/basic-observability.md` | Health, system information, dependency status, SQLite, Redis, RabbitMQ, external finance API checks, and observability summary behavior |
| `docs/project-tracking/week-1-summary.md` | Week 1 progress summary |
| `docs/project-tracking/week-2-database-summary.md` | Week 2 database, Repository Pattern, Watchlist CRUD, soft delete, reactivation, and Swagger verification summary |
| `docs/project-tracking/week-3-external-api-cache-summary.md` | Week 3 external API, Redis cache, quote refresh, PriceSnapshot persistence, snapshot listing, and Swagger verification summary |

---

## Repository Structure

Current documentation and source structure:

    marketinsight-operations-tracker-api/
    ├── docs/
    │   ├── 00-index.md
    │   ├── 01-project-overview.md
    │   ├── final-demo-guide.md
    │   ├── final-technical-summary.md
    │   ├── architecture/
    │   ├── api-contracts/
    │   ├── database-design/
    │   ├── integrations/
    │   ├── configuration/
    │   ├── cache/
    │   ├── messaging/
    │   ├── observability/
    │   └── project-tracking/
    │
    ├── src/
    │   └── MarketInsight.Api/
    │
    ├── docker-compose.yml
    ├── MarketInsight.OperationsTracker.sln
    ├── README.md
    └── .gitignore

---

## Naming Standard

The project uses the following naming baseline:

| Naming Area | Standard Name |
|---|---|
| Domain / Learning Project Name | `MarketInsight Operations Tracker` |
| Repository Display Name | `MarketInsight Operations Tracker API` |
| Repository Name | `marketinsight-operations-tracker-api` |
| Solution Name | `MarketInsight.OperationsTracker` |
| API Project Name | `MarketInsight.Api` |
| Root Namespace | `MarketInsight.Api` |

Repository name uses lowercase kebab-case.

C# project, namespace, class, method, and property names use PascalCase.

API route segments and documentation file names use lowercase kebab-case.

---

## How to Run Locally

Clone the repository:

    git clone https://github.com/yagmurcorum/marketinsight-operations-tracker-api.git

Go to the repository folder:

    cd marketinsight-operations-tracker-api

Restore dependencies:

    dotnet restore

Build the solution:

    dotnet build

Start local dependencies with Docker Compose:

    docker compose up -d

Verify running containers:

    docker ps

Expected containers:

    marketinsight-redis
    marketinsight-rabbitmq

Verify Redis:

    docker exec -it marketinsight-redis redis-cli ping

Expected response:

    PONG

Open RabbitMQ Management UI:

    http://localhost:15672

Default local credentials:

    username: guest
    password: guest

Configure the external finance API key with User Secrets:

    dotnet user-secrets set "FinanceApi:ApiKey" "YOUR_FINANCE_API_KEY" --project src/MarketInsight.Api

Run the API project:

    dotnet run --project src/MarketInsight.Api/MarketInsight.Api.csproj

Open Swagger in the browser using the local URL shown in the terminal.

---

## Database Setup

The project uses SQLite with EF Core Code First.

Local SQLite database files should not be committed to Git.

Ignored database files:

    *.db
    *.db-shm
    *.db-wal

Migration files should be committed because they represent database schema history.

To apply migrations locally:

    dotnet ef database update --project src/MarketInsight.Api/MarketInsight.Api.csproj

---

## Final Demo Preparation

Final demo preparation is supported by:

    docs/final-demo-guide.md
    docs/final-technical-summary.md

The final demo should show:

- Health and dependency endpoints
- Watchlist CRUD behavior
- Sync quote refresh
- Redis cache miss and cache hit behavior
- PriceSnapshot persistence
- Async refresh endpoint
- RabbitMQ publish and consume flow
- Background Worker processing
- Observability summary
- Swagger XML Summary visibility
- Documentation and GitHub tracking

The final demo should focus on understanding:

    What each component does.
    Why each component exists.
    How the request flows through the system.
    How the implementation was tested.
    How the work was documented and tracked.

---

## Development Principles

The project follows these principles:

- Build simple working features first.
- Keep responsibilities separated.
- Use DTOs for API contracts.
- Do not return Entity models directly from API endpoints.
- Keep business rules in the Service layer.
- Keep database access in the Repository layer.
- Keep persistent data in SQLite.
- Use Redis only for short-term quote cache.
- Use RabbitMQ only for asynchronous processing.
- Use Background Worker to consume queued async work.
- Use structured logging placeholders.
- Use CorrelationId to connect async publish and consume logs.
- Use Swagger for endpoint testing.
- Use XML Summary for API documentation.
- Use GitHub Issues and GitHub Projects for tracking.
- Keep README as the repository entry point.
- Keep detailed technical decisions under `docs/`.

---

## Out of Scope

The current MVP does not include:

- User authentication
- User-specific watchlists
- Multiple portfolios
- Trading operations
- Financial advice
- Real-time streaming market data
- Complex investment analytics
- Frontend application
- Mobile application
- Multi-tenant production architecture
- Production-grade security hardening
- Advanced monitoring dashboard
- OpenTelemetry
- Alert evaluation endpoint
- ActionItem creation flow

---

## Summary

MarketInsight Operations Tracker API is a learning-focused backend API project.

It exists to practice professional backend development concepts through a small financial tracking domain.

The repository contains only the backend API component.

The project uses a learning-focused layered monolith architecture and separates responsibilities across Controller, Service, Repository, Entity, DTO, cache, messaging, worker and observability components.

The Watchlist Items API uses normalized symbols, soft delete, and reactivation behavior to keep data consistent while providing clear API behavior.

The quote refresh flow uses an external finance API, Redis cache-aside behavior, and SQLite PriceSnapshot persistence while exposing Swagger-testable watchlist quote endpoints.

The async refresh flow uses RabbitMQ message publishing, Background Worker message consumption, CorrelationId-based logging, and reuse of existing business logic.

The observability endpoints make health, dependency status and project observability features visible through Swagger.

Detailed technical decisions are documented under the `docs/` folder.