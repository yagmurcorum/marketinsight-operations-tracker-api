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

MarketInsight Operations Tracker is a learning-focused backend API that tracks financial symbols, retrieves market quote data, stores price snapshots, evaluates price alerts, and creates operational action items for important market events.

The main domain concepts are:

| Domain Concept | Meaning |
|---|---|
| `WatchlistItem` | A financial symbol tracked by the system |
| `PriceSnapshot` | A saved price record for a tracked symbol |
| `PriceAlert` | A price condition defined for a tracked symbol |
| `ActionItem` | An operational follow-up task created after an alert or important market event |

The main entity is:

    WatchlistItem

All price tracking, alerting, and operational follow-up behavior starts from a tracked financial symbol.

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
- Redis caching
- RabbitMQ asynchronous messaging
- Background Worker processing
- Logging and basic observability
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

## Tech Stack

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
| Documentation | Markdown + XML Summary |
| Tracking | GitHub Issues + GitHub Projects |

---

## Planned MVP Capabilities

The planned MVP includes:

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
- Display Swagger / OpenAPI documentation
- Use XML Summary comments for API documentation
- Track work with GitHub Issues and GitHub Projects

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
| Create watchlist item | POST | `/api/watchlist-items` |
| Delete or deactivate watchlist item | DELETE | `/api/watchlist-items/{symbol}` |

---

## Data Responsibility Standards

| Topic | Standard |
|---|---|
| Entity model | Database / persistence model |
| DTO model | API request / response contract |
| Entity exposure | Entity models should not be returned directly from API endpoints |
| Symbol uniqueness | `NormalizedSymbol` should be unique |
| Symbol normalization | Trim whitespace and convert to uppercase |
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
| Architecture standards | `docs/architecture` |
| API contracts | `docs/api-contracts` |
| Database design | `docs/database-design` |
| Project tracking | `docs/project-tracking` |

Important documentation files:

| Document | Purpose |
|---|---|
| `docs/00-index.md` | Main documentation index |
| `docs/01-project-overview.md` | High-level project overview |
| `docs/architecture/project-naming-standard.md` | Project, repository, solution, namespace, folder, route, and documentation naming standards |
| `docs/architecture/layer-responsibility-standard.md` | Controller, Service, Repository, Entity, and DTO responsibility standards |
| `docs/architecture/api-route-naming-standard.md` | API route naming rules |
| `docs/database-design/entity-relationship-model.md` | Entity relationship model |
| `docs/database-design/entity-constraint-standards.md` | Symbol uniqueness, normalization, decimal, UTC, and persistence standards |
| `docs/database-design/ef-core-sqlite-setup.md` | EF Core and SQLite setup notes |
| `docs/api-contracts/xml-summary-swagger-standard.md` | XML Summary and Swagger documentation standard |
| `docs/api-contracts/api-endpoint-draft.md` | Planned API endpoint draft |
| `docs/project-tracking/week-1-summary.md` | Week 1 progress summary |

---

## Repository Structure

Current documentation and source structure:

    marketinsight-operations-tracker-api/
    ├── docs/
    │   ├── 00-index.md
    │   ├── 01-project-overview.md
    │   ├── architecture/
    │   ├── api-contracts/
    │   ├── database-design/
    │   └── project-tracking/
    │
    ├── src/
    │   └── MarketInsight.Api/
    │
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

---

## Summary

MarketInsight Operations Tracker API is a learning-focused backend API project.

It exists to practice professional backend development concepts through a small financial tracking domain.

The repository contains only the backend API component.

The project uses a learning-focused layered monolith architecture and separates responsibilities across Controller, Service, Repository, Entity, and DTO layers.

Detailed technical decisions are documented under the `docs/` folder.