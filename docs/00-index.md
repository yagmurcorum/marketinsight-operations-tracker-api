# Documentation Index

This document is the main entry point for the technical documentation of the MarketInsight Operations Tracker API.

The purpose of this index is to help readers quickly locate project documentation by topic.

---

## Documentation Structure

The project documentation is organized by topic.

```text
docs/
├── 00-index.md
├── 01-project-overview.md
│
├── architecture/
│   ├── project-naming-standard.md
│   ├── http-rest-lifecycle.md
│   ├── layer-responsibility-standard.md
│   ├── api-route-naming-standard.md
│   ├── repository-pattern-and-linq.md
│   └── service-layer-and-quote-refresh-flow.md
│
├── api-contracts/
│   ├── xml-summary-swagger-standard.md
│   ├── api-endpoint-draft.md
│   ├── watchlist-items-api-contract.md
│   ├── quote-refresh-api-contract.md
│   └── watchlist-quote-endpoints-api-contract.md
│
├── database-design/
│   ├── entity-design.md
│   ├── ef-core-sqlite-setup.md
│   ├── entity-relationship-model.md
│   └── entity-constraint-standards.md
│
├── integrations/
│   └── public-finance-api-integration.md
│
├── configuration/
│   └── user-secrets-and-strategy-pattern.md
│
├── cache/
│   └── redis-cache-design.md
│
└── project-tracking/
    ├── week-1-summary.md
    ├── week-2-database-summary.md
    └── week-3-external-api-cache-summary.md
```

---

## Project Overview

| Document | Purpose |
|---|---|
| `docs/01-project-overview.md` | Explains the project purpose, MVP scope, learning goal, and high-level project context |

---

## Architecture Documentation

| Document | Purpose |
|---|---|
| `docs/architecture/project-naming-standard.md` | Defines project, repository, solution, API project, namespace, folder, route, and documentation naming standards |
| `docs/architecture/http-rest-lifecycle.md` | Explains the HTTP/REST request-response lifecycle and how it relates to the API |
| `docs/architecture/layer-responsibility-standard.md` | Defines Controller, Service, Repository, Entity, and DTO responsibilities |
| `docs/architecture/api-route-naming-standard.md` | Defines API route naming rules used by the project |
| `docs/architecture/repository-pattern-and-linq.md` | Explains Repository Pattern, LINQ usage, async EF Core queries, and data access separation |
| `docs/architecture/service-layer-and-quote-refresh-flow.md` | Documents the service-layer boundary and quote refresh flow design |

---

## API Contract Documentation

| Document | Purpose |
|---|---|
| `docs/api-contracts/xml-summary-swagger-standard.md` | Defines XML Summary and Swagger documentation standards |
| `docs/api-contracts/api-endpoint-draft.md` | Documents the initial controller endpoint draft and the `GET /api/system/info` endpoint |
| `docs/api-contracts/watchlist-items-api-contract.md` | Defines Watchlist Items CRUD endpoints, request and response models, status codes, soft delete behavior, reactivation behavior, and Swagger test flow |
| `docs/api-contracts/quote-refresh-api-contract.md` | Defines the quote refresh service contract, cache-aside flow, controlled result behavior, and PriceSnapshot persistence rules |
| `docs/api-contracts/watchlist-quote-endpoints-api-contract.md` | Defines Swagger-testable watchlist quote refresh and snapshot listing endpoints |

---

## Database Design Documentation

| Document | Purpose |
|---|---|
| `docs/database-design/entity-design.md` | Explains Entity and DTO model design, including WatchlistItem lifecycle, soft delete, and reactivation behavior |
| `docs/database-design/ef-core-sqlite-setup.md` | Documents SQLite, EF Core, AppDbContext, migration, and database setup |
| `docs/database-design/entity-relationship-model.md` | Explains relationships between WatchlistItem, PriceSnapshot, PriceAlert, and ActionItem |
| `docs/database-design/entity-constraint-standards.md` | Defines entity-level standards such as symbol uniqueness, normalization, soft delete, reactivation, decimal usage, and UTC date fields |

---

## Integration Documentation

| Document | Purpose |
|---|---|
| `docs/integrations/public-finance-api-integration.md` | Documents the external finance API client flow, external response model, internal quote DTO, and provider-facing quote retrieval behavior |

---

## Configuration Documentation

| Document | Purpose |
|---|---|
| `docs/configuration/user-secrets-and-strategy-pattern.md` | Documents local API key handling with User Secrets and the simple quote provider abstraction |

---

## Cache Documentation

| Document | Purpose |
|---|---|
| `docs/cache/redis-cache-design.md` | Documents Redis cache responsibility, cache-aside behavior, `quote:{symbol}` key format, 5-minute TTL, and local Docker Compose setup |

---

## Project Tracking Documentation

| Document | Purpose |
|---|---|
| `docs/project-tracking/week-1-summary.md` | Summarizes Week 1 implementation progress and verification results |
| `docs/project-tracking/week-2-database-summary.md` | Summarizes database setup, Repository Pattern, Watchlist Items CRUD implementation, soft delete behavior, reactivation behavior, Swagger demo flow, and Week 2 verification results |
| `docs/project-tracking/week-3-external-api-cache-summary.md` | Summarizes external finance API integration, User Secrets, Redis cache, quote refresh flow, PriceSnapshot persistence, snapshot listing, and Swagger verification |

---

## Recommended Reading Order

Use this documentation index as the starting point when navigating the project.

Recommended order:

1. Start with `docs/01-project-overview.md`.
2. Review architecture standards under `docs/architecture`.
3. Review API contracts under `docs/api-contracts`.
4. Review database and entity design under `docs/database-design`.
5. Review integration, configuration, and cache documents.
6. Review weekly progress summaries under `docs/project-tracking`.

---

## Documentation Principle

The README file should remain the main repository entry point.

Detailed technical decisions should be documented under the relevant `docs/` subfolders.

This keeps the repository easier to understand, maintain, and extend as the project grows.