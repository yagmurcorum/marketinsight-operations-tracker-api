# Documentation Index

This document is the main entry point for the technical documentation of the MarketInsight Operations Tracker API.

The purpose of this index is to help readers quickly understand where each documentation topic is located and which document should be read for each project area.

---

## Documentation Structure

The project documentation is organized by topic.

    docs/
    ├── 00-index.md
    ├── 01-project-overview.md
    │
    ├── architecture/
    │   ├── project-naming-standard.md
    │   ├── http-rest-lifecycle.md
    │   ├── layer-responsibility-standard.md
    │   └── api-route-naming-standard.md
    │
    ├── api-contracts/
    │   ├── xml-summary-swagger-standard.md
    │   └── api-endpoint-draft.md
    │
    ├── database-design/
    │   ├── entity-design.md
    │   ├── ef-core-sqlite-setup.md
    │   ├── entity-relationship-model.md
    │   └── entity-constraint-standards.md
    │
    └── project-tracking/
        └── week-1-summary.md

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
| `docs/architecture/layer-responsibility-standard.md` | Defines the responsibilities of Controller, Service, Repository, Entity, and DTO layers |
| `docs/architecture/api-route-naming-standard.md` | Defines API route naming rules before Watchlist CRUD implementation |

---

## API Contract Documentation

| Document | Purpose |
|---|---|
| `docs/api-contracts/xml-summary-swagger-standard.md` | Defines XML Summary and Swagger documentation standards |
| `docs/api-contracts/api-endpoint-draft.md` | Lists planned API endpoints and API contract decisions |

---

## Database Design Documentation

| Document | Purpose |
|---|---|
| `docs/database-design/entity-design.md` | Explains the initial Entity and DTO model design |
| `docs/database-design/ef-core-sqlite-setup.md` | Documents SQLite, EF Core, AppDbContext, migration, and database setup |
| `docs/database-design/entity-relationship-model.md` | Explains relationships between WatchlistItem, PriceSnapshot, PriceAlert, and ActionItem |
| `docs/database-design/entity-constraint-standards.md` | Defines entity-level standards such as Symbol uniqueness, normalization, decimal usage, and UTC date fields |

---

## Project Tracking Documentation

| Document | Purpose |
|---|---|
| `docs/project-tracking/week-1-summary.md` | Summarizes Week 1 implementation progress and verification results |

---

## How to Use This Documentation

Use this documentation index as the starting point when navigating the project.

Recommended reading order:

1. Start with `docs/01-project-overview.md`.
2. Review architecture standards under `docs/architecture`.
3. Review API contract decisions under `docs/api-contracts`.
4. Review database and entity design under `docs/database-design`.
5. Review project progress under `docs/project-tracking`.

---

## Documentation Principle

The README file should remain the main repository entry point.

Detailed technical decisions should be documented under the relevant `docs/` subfolders.

This keeps the repository easier to understand, maintain, and extend as the project grows.