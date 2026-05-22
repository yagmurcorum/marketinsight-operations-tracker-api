# MarketInsight Operations Tracker API

MarketInsight Operations Tracker API is a learning-focused .NET backend project designed for preparation.

The project aims to practice backend development fundamentals through a small but realistic MVP that includes API design, documentation, persistence, caching, asynchronous processing, logging, and basic observability.

---

## Project Goal

The main goal of this project is not to build a production-grade financial platform.

The goal is to learn how a backend API is structured, developed, tested, documented, and tracked in a professional workflow.

This project follows a 4-week MVP development plan.

---

## MVP Scope

At the end of the 4-week plan, the API should be able to:

- Manage a financial symbol watchlist
- Retrieve quote data from a public finance API
- Cache quote data in Redis
- Log cache hit and cache miss behavior
- Persist price snapshots in SQLite
- Send async price refresh messages to RabbitMQ
- Process queue messages with a Background Worker
- Provide health check and dependency status endpoints
- Display Swagger/OpenAPI documentation
- Use XML Summary comments for API documentation
- Track work with GitHub Issues and GitHub Projects

---

## Current Phase

Current phase:

    Week 1 - Project Setup, REST API Basics, Swagger, XML Summary, and Documentation Foundation

Current focus:

- ASP.NET Core Web API project structure
- Controller-based request handling
- Swagger/OpenAPI endpoint verification
- Initial health endpoint implementation
- Initial technical documentation structure
- Clean repository structure

---

## Tech Stack

| Area | Technology |
|---|---|
| Backend Framework | ASP.NET Core Web API |
| Runtime | .NET 8 |
| Language | C# |
| API Testing | Swagger / OpenAPI |
| Persistent Database | SQLite |
| ORM | EF Core Code First |
| Cache | Redis |
| Messaging | RabbitMQ |
| Background Processing | Background Worker |
| Documentation | Markdown + XML Summary |
| Tracking | GitHub Issues + GitHub Projects |

---

## Core Architecture Decisions

The project follows a simple learning-focused backend architecture.

Key decisions:

- ASP.NET Core Web API will be used as the main backend project.
- Controllers will handle HTTP request and response flow.
- Services will contain business logic.
- Repositories will handle database access.
- DTOs will be used for API request and response models.
- Entities will represent database persistence models.
- SQLite will be used as the persistent database.
- Redis will only be used for short-term quote caching.
- RabbitMQ will only be used for async price refresh messaging.
- Swagger will be used for API testing and documentation.
- GitHub Issues and GitHub Projects will be used instead of Jira.

---

## Implemented Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/health` | Returns basic API health information |

The `GET /api/health` endpoint verifies that the API is running and reachable through Swagger.

---

## Documentation

Project documentation is maintained under the `docs/` folder.

Current documentation files:

- `docs/01-project-overview.md`
- `docs/02-http-rest-lifecycle.md`

---

## Planned Repository Structure

    MarketInsight.OperationsTracker/
    │
    ├── src/
    │   └── MarketInsight.Api/
    │       ├── Controllers/
    │       ├── Services/
    │       ├── Repositories/
    │       ├── Data/
    │       ├── Entities/
    │       ├── DTOs/
    │       ├── ExternalServices/
    │       ├── Cache/
    │       ├── Messaging/
    │       ├── BackgroundWorkers/
    │       ├── Observability/
    │       ├── Middleware/
    │       └── Program.cs
    │
    ├── docs/
    │   ├── business-analysis/
    │   ├── requirements/
    │   ├── architecture/
    │   ├── api-contracts/
    │   ├── database-design/
    │   ├── redis-design/
    │   ├── rabbitmq-design/
    │   ├── observability/
    │   └── project-tracking/
    │
    ├── tests/
    │   └── MarketInsight.Api.Tests/
    │
    ├── .github/
    │   ├── ISSUE_TEMPLATE/
    │   └── pull_request_template.md
    │
    ├── README.md
    ├── CHANGELOG.md
    ├── .gitignore
    └── MarketInsight.OperationsTracker.sln

Note:

The full folder structure will be created gradually as the project progresses. Empty folders will not be added unnecessarily.

---

## Current Repository Structure

    marketinsight-operations-tracker-api/
    │
    ├── .github/
    │   └── copilot-instructions.md
    │
    ├── docs/
    │   ├── 01-project-overview.md
    │   └── 02-http-rest-lifecycle.md
    │
    ├── src/
    │   └── MarketInsight.Api/
    │       ├── Controllers/
    │       │   └── HealthController.cs
    │       ├── Properties/
    │       ├── appsettings.Development.json
    │       ├── appsettings.json
    │       ├── MarketInsight.Api.csproj
    │       ├── MarketInsight.Api.http
    │       └── Program.cs
    │
    ├── .gitignore
    ├── README.md
    └── MarketInsight.OperationsTracker.sln

---

## Week 1 Goals

By the end of Week 1, the project should include:

- A clean GitHub repository
- A configured GitHub Project Board
- A working ASP.NET Core Web API project
- Swagger/OpenAPI enabled
- Initial XML Summary setup
- Basic API endpoints:
  - `GET /api/health`
  - `GET /api/system/info`
- Initial technical documentation

Current Week 1 progress:

- GitHub repository and project board are configured.
- ASP.NET Core Web API project is created.
- Swagger/OpenAPI is enabled.
- `GET /api/health` endpoint is implemented and verified.
- Initial project documentation is started.

---

## Initial Setup Notes

The ASP.NET Core Web API project was created with the following setup decisions:

- Framework: `.NET 8`
- Authentication: `None`
- HTTPS: `Enabled`
- Container support: `Disabled`
- OpenAPI / Swagger support: `Enabled`
- Controllers: `Enabled`
- .NET Aspire: `Disabled`

These decisions support the learning goal of building a controller-based Web API that can be tested through Swagger.

Docker-related setup will be introduced later for Redis and RabbitMQ using Docker Compose.

---

## Work Tracking

This project uses GitHub Issues and GitHub Projects for tracking.

Main board:

    MarketInsight Operations Tracker API Board

Board workflow:

    Backlog → Ready → In Progress → Review → Testing → Done

Blocked work is tracked separately using the `Blocked` status.

---

## Development Principle

The project follows this principle:

    First build a simple working structure, then improve it gradually.

The focus is on understanding backend development concepts step by step without adding unnecessary complexity too early.
