# Project Overview

This document provides the high-level overview of the MarketInsight Operations Tracker API.

The purpose of this document is to explain what this repository represents, why the project exists, what the implemented MVP scope includes, and how the project is structured from a learning and architecture perspective.

---

## Repository Scope

This repository contains the backend API component of the MarketInsight Operations Tracker learning project.

This repository represents:

- A learning-focused backend API
- A .NET 8 ASP.NET Core Web API project
- A backend practice project for API design, persistence, caching, messaging, logging, observability, and documentation

This repository does not represent:

- A frontend application
- A mobile application
- A complete commercial product
- A production-grade financial platform
- A real trading or portfolio management system
- A system that provides financial advice

The project is designed for backend learning and technical practice.

---

## Project Name

The project naming standard is:

| Naming Area | Standard Name |
|---|---|
| Domain / Learning Project Name | `MarketInsight Operations Tracker` |
| Repository Display Name | `MarketInsight Operations Tracker API` |
| Repository Name | `marketinsight-operations-tracker-api` |
| Solution Name | `MarketInsight.OperationsTracker` |
| API Project Name | `MarketInsight.Api` |
| Root Namespace | `MarketInsight.Api` |

The repository name includes `api` because this repository contains only the backend API component.

---

## Domain Definition

MarketInsight Operations Tracker is a learning-focused backend API that tracks financial symbols, retrieves market quote data, stores price snapshots, and prepares a foundation for future operational alert and action workflows.

The domain is centered around four main concepts:

| Domain Concept | Meaning |
|---|---|
| `WatchlistItem` | A financial symbol tracked by the system |
| `PriceSnapshot` | A saved price record for a tracked symbol |
| `PriceAlert` | A future price condition concept for a tracked symbol |
| `ActionItem` | A future operational follow-up concept after an alert or important market event |

The main implemented runtime flow currently focuses on:

- WatchlistItem management.
- Quote refresh.
- PriceSnapshot persistence.
- Redis cache behavior.
- RabbitMQ async refresh processing.
- Basic observability.

The main domain entity is:

    WatchlistItem

All price tracking and refresh behavior starts from a tracked financial symbol.

PriceAlert and ActionItem are part of the broader domain model, but they should not be presented as completed runtime API flows unless implemented later.

---

## Project Goal

The main goal of this project is not to build a production-grade financial product.

The goal is to learn how a backend API is structured, developed, tested, documented, and tracked in a professional workflow.

The project focuses on practicing:

- ASP.NET Core Web API fundamentals
- HTTP and REST API design
- Controller-based API development
- XML Summary and Swagger documentation
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

- The project is developed as a single backend API application.
- The code is separated into clear responsibility layers.
- The goal is not to build a distributed microservice system.
- The goal is to learn backend responsibility separation in a professional API project.

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

## High-Level Application Flow

Typical request flow:

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

Typical response flow:

    SQLite Database
          ↓
    Entity
          ↓
    Repository
          ↓
    Service
          ↓
    DTO
          ↓
    Controller
          ↓
    Client / Swagger

This structure keeps API behavior, business logic, database access, and data contracts separated.

---

## Implemented MVP Scope

The current MVP includes:

- Manage financial symbol watchlist items.
- List active watchlist items.
- Create new watchlist items.
- Reject active duplicate symbols.
- Reactivate inactive watchlist items.
- Soft delete watchlist items.
- Retrieve quote data from a public finance API.
- Configure the external finance API key with User Secrets.
- Cache quote data in Redis.
- Log cache hit and cache miss behavior.
- Persist PriceSnapshot records in SQLite.
- List saved PriceSnapshot records by symbol.
- Refresh quote data synchronously.
- Send asynchronous price refresh messages to RabbitMQ.
- Process queued refresh messages with a Background Worker.
- Reuse QuoteRefreshService in both sync and async refresh flows.
- Use structured logging placeholders.
- Carry CorrelationId through the async RabbitMQ flow.
- Provide basic health, system, dependency and observability endpoints.
- Display Swagger/OpenAPI documentation.
- Use XML Summary comments for API documentation.
- Track work with GitHub Issues and GitHub Projects.
- Maintain Markdown technical documentation under the `docs/` folder.

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

These topics may be considered only as future enhancements.

---

## Development Approach

The project is developed incrementally.

Each implementation step focuses on building a small, working backend capability and then documenting the related technical decisions.

The project follows this learning order:

1. API foundation
2. HTTP and REST lifecycle
3. Swagger and XML Summary documentation
4. Entity and DTO model foundation
5. SQLite and EF Core persistence setup
6. Architecture and documentation alignment
7. Repository Pattern and LINQ queries
8. Watchlist CRUD endpoints
9. Public finance API integration
10. User Secrets configuration
11. Redis caching
12. Quote refresh flow
13. PriceSnapshot persistence
14. Snapshot listing
15. RabbitMQ messaging
16. Async refresh endpoint
17. Background Worker processing
18. Structured logging and correlation
19. Basic observability endpoints
20. Final demo and technical documentation preparation

Detailed weekly progress is tracked separately under:

    docs/project-tracking

Current weekly summaries:

- `docs/project-tracking/week-1-summary.md`
- `docs/project-tracking/week-2-database-summary.md`
- `docs/project-tracking/week-3-external-api-cache-summary.md`
- `docs/project-tracking/week-4-async-observability-summary.md`

This keeps the project overview stable while allowing progress documentation to evolve over time.

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

Watchlist CRUD routes:

| Operation | HTTP Method | Route |
|---|---|---|
| List watchlist items | GET | `/api/watchlist-items` |
| Get watchlist item by symbol | GET | `/api/watchlist-items/{symbol}` |
| Create or reactivate watchlist item | POST | `/api/watchlist-items` |
| Delete or deactivate watchlist item | DELETE | `/api/watchlist-items/{symbol}` |

Watchlist quote and snapshot routes:

| Operation | HTTP Method | Route |
|---|---|---|
| Refresh quote data for a symbol synchronously | POST | `/api/watchlist-items/{symbol}/refresh` |
| List saved price snapshots for a symbol | GET | `/api/watchlist-items/{symbol}/snapshots` |
| Queue quote refresh for a symbol asynchronously | POST | `/api/watchlist-items/{symbol}/refresh-async` |

Health, system and observability routes:

| Operation | HTTP Method | Route |
|---|---|---|
| Check API health | GET | `/api/health` |
| Get system information | GET | `/api/system/info` |
| Check dependency status | GET | `/api/system/dependencies` |
| Get observability summary | GET | `/api/observability/summary` |

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

This keeps the API behavior clear while preserving data consistency.

---

## Synchronous Quote Refresh Flow

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

## Asynchronous Quote Refresh Flow

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

## Core Technical Decisions

| Area | Decision |
|---|---|
| Backend framework | ASP.NET Core Web API |
| Runtime | .NET 8 |
| Language | C# |
| API documentation | Swagger / OpenAPI |
| XML API documentation | XML Summary |
| Persistent database | SQLite |
| ORM | EF Core Code First |
| Cache | Redis for short-term quote cache only |
| Messaging | RabbitMQ for async refresh queue only |
| Background processing | Background Worker |
| Configuration | appsettings + User Secrets |
| Documentation | Markdown |
| Tracking | GitHub Issues + GitHub Projects |

---

## Data Responsibility Decisions

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

## Documentation Map

Detailed documentation is organized under the `docs/` folder.

Main documentation entry point:

    docs/00-index.md

Key documentation areas:

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

Project tracking summaries:

| Document | Purpose |
|---|---|
| `docs/project-tracking/week-1-summary.md` | Week 1 implementation progress and verification |
| `docs/project-tracking/week-2-database-summary.md` | Week 2 database, repository, watchlist CRUD and verification |
| `docs/project-tracking/week-3-external-api-cache-summary.md` | Week 3 external finance API, Redis cache, quote refresh, PriceSnapshot and snapshot listing verification |
| `docs/project-tracking/week-4-async-observability-summary.md` | Week 4 RabbitMQ async processing, Background Worker, structured logging, CorrelationId and basic observability verification |

The README should remain the main repository entry point.

Detailed technical decisions should be documented under the relevant `docs/` subfolders.

---

## Final Demo Preparation

Final demo preparation is supported by:

    docs/final-demo-guide.md
    docs/final-technical-summary.md

The final demo should show:

- Health and dependency endpoints.
- Watchlist CRUD behavior.
- Sync quote refresh.
- Redis cache miss and cache hit behavior.
- PriceSnapshot persistence.
- Async refresh endpoint.
- RabbitMQ publish and consume flow.
- Background Worker processing.
- Observability summary.
- Swagger XML Summary visibility.
- Documentation and GitHub tracking.

The final demo should focus on understanding:

    What each component does.
    Why each component exists.
    How the request flows through the system.
    How the implementation was tested.
    How the work was documented and tracked.

---

## Development Principle

The project follows this development principle:

    First build a simple working structure, then improve it gradually.

The focus is on understanding backend development concepts step by step without adding unnecessary complexity too early.

This project prioritizes:

- Clarity
- Consistency
- Testability
- Documentation
- Maintainability
- Step-by-step learning

---

## Summary

MarketInsight Operations Tracker API is a learning-focused backend API project.

It exists to practice professional backend development concepts through a small financial tracking domain.

The repository contains only the backend API component.

The project uses a learning-focused layered monolith architecture and separates responsibilities across Controller, Service, Repository, Entity, DTO, cache, messaging, worker and observability components.

The Watchlist Items API uses normalized symbols, soft delete, and reactivation behavior to keep data consistent while providing clear API behavior.

The quote refresh flow demonstrates external finance API integration, Redis cache-aside behavior, and SQLite PriceSnapshot persistence.

The async refresh flow demonstrates RabbitMQ message publishing, Background Worker message consumption, CorrelationId-based logging, and reuse of existing business logic.

The observability endpoints make health, dependency status and project observability features visible through Swagger.

The development approach is incremental, documented, and aligned with backend learning goals.