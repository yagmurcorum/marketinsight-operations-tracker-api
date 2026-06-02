# Project Overview

This document provides the high-level overview of the MarketInsight Operations Tracker API.

The purpose of this document is to explain what this repository represents, why the project exists, what the MVP scope includes, and how the project is structured from a learning and architecture perspective.

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

MarketInsight Operations Tracker is a learning-focused backend API that tracks financial symbols, retrieves market quote data, stores price snapshots, evaluates price alerts, and creates operational action items for important market events.

The domain is centered around four main concepts:

| Domain Concept | Meaning |
|---|---|
| `WatchlistItem` | A financial symbol tracked by the system |
| `PriceSnapshot` | A saved price record for a tracked symbol |
| `PriceAlert` | A price condition defined for a tracked symbol |
| `ActionItem` | An operational follow-up task created after an alert or important market event |

The main domain entity is:

    WatchlistItem

All price tracking, alerting, and operational follow-up behavior starts from a tracked financial symbol.

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
- Redis cache behavior
- RabbitMQ asynchronous messaging
- Background Worker processing
- Logging and basic observability
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

Main layers:

| Layer | Responsibility |
|---|---|
| Controller | Handles HTTP request and response flow |
| Service | Handles use cases and business rules |
| Repository | Handles database access |
| Entity | Represents database persistence model |
| DTO | Represents API request and response contract |

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

## MVP Scope

At the end of the 4-week MVP plan, the API should be able to:

- Manage financial symbol watchlist items
- Retrieve quote data from a public finance API
- Cache quote data in Redis
- Log cache hit and cache miss behavior
- Persist price snapshots in SQLite
- Send asynchronous price refresh messages to RabbitMQ
- Process queue messages with a Background Worker
- Evaluate basic price alert rules
- Create operational action items
- Provide health check and dependency status endpoints
- Display Swagger/OpenAPI documentation
- Use XML Summary comments for API documentation
- Track work with GitHub Issues and GitHub Projects

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
10. Redis caching
11. RabbitMQ messaging and Background Worker processing
12. Logging and basic observability

Detailed weekly progress is tracked separately under:

    docs/project-tracking

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

Planned Watchlist CRUD routes:

| Operation | HTTP Method | Route |
|---|---|---|
| List watchlist items | GET | `/api/watchlist-items` |
| Get watchlist item by symbol | GET | `/api/watchlist-items/{symbol}` |
| Create or reactivate watchlist item | POST | `/api/watchlist-items` |
| Delete or deactivate watchlist item | DELETE | `/api/watchlist-items/{symbol}` |

Additional watchlist quote routes:

| Operation | HTTP Method | Route |
|---|---|---|
| Refresh quote data for a symbol | POST | `/api/watchlist-items/{symbol}/refresh` |
| List saved price snapshots for a symbol | GET | `/api/watchlist-items/{symbol}/snapshots` |

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

## Core Technical Decisions

| Area | Decision |
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
| Documentation | Markdown + XML Summary |
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
| Architecture standards | `docs/architecture` |
| API contracts | `docs/api-contracts` |
| Database design | `docs/database-design` |
| External integrations | `docs/integrations` |
| Configuration | `docs/configuration` |
| Cache design | `docs/cache` |
| Project tracking | `docs/project-tracking` |

The README should remain the main repository entry point.

Detailed technical decisions should be documented under the relevant `docs/` subfolders.

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

The project uses a learning-focused layered monolith architecture and separates responsibilities across Controller, Service, Repository, Entity, and DTO layers.

The Watchlist Items API uses normalized symbols, soft delete, and reactivation behavior to keep data consistent while providing clear API behavior.

The development approach is incremental, documented, and aligned with backend learning goals.