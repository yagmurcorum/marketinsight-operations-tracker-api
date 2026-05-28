# Project Naming Standard

This document defines the naming standard for the MarketInsight Operations Tracker API repository.

The purpose of this standard is to keep the project name, repository name, solution name, API project name, namespaces, folders, and documentation references consistent as the project grows.

---

## Purpose

A clear naming standard helps the project stay understandable and maintainable.

This standard answers the following questions:

- What is the main domain/project name?
- What does this repository represent?
- What is the repository name?
- What is the solution name?
- What is the API project name?
- How should namespaces be named?
- How should folders and documentation references be named?
- Which naming patterns should be avoided?

---

## Repository Scope

This repository represents only the backend API component of the MarketInsight Operations Tracker learning project.

This repository does not represent:

- A frontend application
- A mobile application
- A complete commercial product
- A production-grade financial trading platform
- A real trading or portfolio management system

This repository represents:

- A learning-focused backend API
- A .NET 8 ASP.NET Core Web API project
- A backend practice project for API design, persistence, caching, messaging, logging, observability, and documentation

---

## Main Naming Decision

| Naming Area | Standard Name |
|---|---|
| Domain / Learning Project Name | `MarketInsight Operations Tracker` |
| Repository Display Name | `MarketInsight Operations Tracker API` |
| Repository Name | `marketinsight-operations-tracker-api` |
| Solution Name | `MarketInsight.OperationsTracker` |
| API Project Name | `MarketInsight.Api` |
| API Assembly Name | `MarketInsight.Api` |
| Root Namespace | `MarketInsight.Api` |

---

## Domain Name

The main domain / learning project name is:

    MarketInsight Operations Tracker

This name represents the business/domain idea of the project.

The domain idea is:

    Track financial symbols, retrieve market quote data, store price snapshots, evaluate price alerts, and create operational follow-up actions for important market events.

The domain name should be used when describing the overall project concept.

Example usage:

    MarketInsight Operations Tracker is a learning-focused backend API for financial symbol tracking and operational follow-up workflows.

---

## Repository Display Name

The repository display name is:

    MarketInsight Operations Tracker API

This name makes it clear that the repository contains the API component.

Reason:

    MarketInsight Operations Tracker describes the domain.
    API clarifies that this repository contains the backend API implementation.

---

## Repository Name

The GitHub repository name is:

    marketinsight-operations-tracker-api

Repository naming rules:

- Use lowercase letters.
- Use kebab-case.
- Use hyphens between words.
- Include `api` because this repository represents the backend API component.
- Avoid spaces.
- Avoid PascalCase in repository slug.
- Avoid overly generic names.

Correct:

    marketinsight-operations-tracker-api

Avoid:

    MarketInsightOperationsTracker
    MarketInsight.Api
    marketinsight
    operations-tracker
    marketinsight-project

---

## Solution Name

The .NET solution name is:

    MarketInsight.OperationsTracker

Reason:

- Solution names usually use PascalCase with dot-separated segments.
- The solution represents the broader backend solution boundary.
- It does not need to include `Api` because the solution may later contain tests or additional related projects.

Example current solution file:

    MarketInsight.OperationsTracker.sln

---

## API Project Name

The API project name is:

    MarketInsight.Api

Reason:

- The current repository contains the backend API project.
- The name is concise and clear.
- The project is the executable ASP.NET Core Web API application.
- The root namespace stays simple for beginner-level development.

Example project file:

    src/MarketInsight.Api/MarketInsight.Api.csproj

---

## Why Solution and API Project Names Are Different

The solution name and API project name do not need to be identical.

The solution represents the overall .NET solution:

    MarketInsight.OperationsTracker

The API project represents the actual Web API application:

    MarketInsight.Api

This allows the solution to grow later.

Possible future structure:

    MarketInsight.OperationsTracker.sln
        ├── MarketInsight.Api
        └── MarketInsight.Api.Tests

---

## Namespace Naming Standard

The root namespace should follow the API project name:

    MarketInsight.Api

Recommended namespace examples:

| Area | Namespace |
|---|---|
| Controllers | `MarketInsight.Api.Controllers` |
| Data | `MarketInsight.Api.Data` |
| Entities | `MarketInsight.Api.Entities` |
| DTOs | `MarketInsight.Api.DTOs` |
| Watchlist DTOs | `MarketInsight.Api.DTOs.Watchlist` |
| Repositories | `MarketInsight.Api.Repositories` |
| Services | `MarketInsight.Api.Services` |
| External services | `MarketInsight.Api.ExternalServices` |
| Cache | `MarketInsight.Api.Cache` |
| Messaging | `MarketInsight.Api.Messaging` |
| Background workers | `MarketInsight.Api.BackgroundWorkers` |
| Observability | `MarketInsight.Api.Observability` |

---

## Folder Naming Standard

Project folders should use PascalCase for C# source folders.

Preferred source folders:

    Controllers
    Services
    Repositories
    Data
    Entities
    DTOs
    ExternalServices
    Cache
    Messaging
    BackgroundWorkers
    Observability
    Middleware

Reason:

    These folders map to C# namespaces and application layers.

---

## Documentation Folder Naming Standard

Documentation folders should use kebab-case.

Preferred documentation folders:

    docs/architecture
    docs/api-contracts
    docs/database-design
    docs/project-tracking
    docs/redis-design
    docs/rabbitmq-design
    docs/observability

Reason:

    Documentation paths behave more like URLs and file-system documentation categories.
    Kebab-case keeps names readable and consistent.

---

## Documentation File Naming Standard

Documentation files should use lowercase kebab-case.

Preferred:

    layer-responsibility-standard.md
    api-route-naming-standard.md
    entity-relationship-model.md
    entity-constraint-standards.md
    ef-core-sqlite-setup.md

Avoid:

    LayerResponsibilityStandard.md
    layer_responsibility_standard.md
    Layer Responsibility Standard.md

---

## API Route Naming Standard Reference

API route names should follow the route naming standard documented in:

    docs/architecture/api-route-naming-standard.md

Route segments should use lowercase kebab-case.

Example:

    /api/watchlist-items
    /api/price-snapshots
    /api/price-alerts
    /api/action-items

---

## Domain Concept Naming

Domain concepts should use clear PascalCase names in C# code.

| Domain Concept | C# Name | API Route Segment |
|---|---|---|
| Watchlist item | `WatchlistItem` | `watchlist-items` |
| Price snapshot | `PriceSnapshot` | `price-snapshots` |
| Price alert | `PriceAlert` | `price-alerts` |
| Action item | `ActionItem` | `action-items` |

---

## Entity Naming Standard

Entity class names should be singular PascalCase nouns.

Preferred:

    WatchlistItem
    PriceSnapshot
    PriceAlert
    ActionItem

Avoid:

    WatchlistItems
    PriceSnapshots
    Alert
    Action

Reason:

    An Entity class represents one database record.

---

## DTO Naming Standard

DTO names should describe their API purpose clearly.

Request DTOs should describe what the API receives.

Response DTOs should describe what the API returns.

Preferred:

    CreateWatchlistItemRequest
    WatchlistItemResponse

Future examples:

    CreatePriceAlertRequest
    PriceAlertResponse
    ActionItemResponse

Avoid:

    WatchlistItemDto
    WatchlistRequest
    DataDto
    ModelDto

Reason:

    DTO names should make the API contract purpose clear.

---

## Controller Naming Standard

Controller names should describe the API resource they manage.

Preferred:

    WatchlistItemsController

Reason:

    The controller manages WatchlistItem resources.

Avoid:

    WatchlistController

Reason:

    It can imply that the controller manages the watchlist container itself rather than the items inside the watchlist.

---

## Service Naming Standard

Service names should describe the use case or domain area they manage.

Preferred future examples:

    IWatchlistItemService
    WatchlistItemService

or, if the scope remains simple:

    IWatchlistService
    WatchlistService

Decision for current MVP:

    Use WatchlistItem naming when the service is specifically focused on watchlist item operations.

---

## Repository Naming Standard

Repository names should describe the entity or aggregate they access.

Preferred future examples:

    IWatchlistItemRepository
    WatchlistItemRepository

or, if the project keeps the shorter domain naming:

    IWatchlistRepository
    WatchlistRepository

Decision for current MVP:

    Prefer WatchlistItem naming when aligning directly with the `WatchlistItem` entity and `/api/watchlist-items` route.

---

## Method Naming Standard

Public async methods should use clear names and the `Async` suffix.

Preferred examples:

    GetWatchlistItemsAsync
    GetWatchlistItemBySymbolAsync
    CreateWatchlistItemAsync
    DeleteWatchlistItemAsync

Repository method examples:

    GetAllActiveAsync
    GetByNormalizedSymbolAsync
    ExistsByNormalizedSymbolAsync
    AddAsync
    SaveChangesAsync

Avoid:

    Get
    Add
    Delete
    DoWork
    HandleData

---

## Naming Consistency Rules

The project should follow these consistency rules:

| Area | Naming Style |
|---|---|
| Repository slug | lowercase kebab-case |
| Solution name | PascalCase with dot-separated segments |
| Project name | PascalCase with dot-separated segments |
| Namespace | PascalCase with dot-separated segments |
| C# class | PascalCase |
| C# method | PascalCase |
| C# property | PascalCase |
| Private field | `_camelCase` |
| API route segment | lowercase kebab-case |
| Documentation folder | lowercase kebab-case |
| Documentation file | lowercase kebab-case |

---

## Naming Anti-Patterns

Avoid inconsistent naming such as:

    MarketInsightApi
    MarketInsight.API
    marketInsightOperationsTracker
    marketinsight_operations_tracker_api
    WatchlistController for watchlist item resources
    /api/watchlistItems
    /api/get-watchlist-items
    WatchlistItemDto for every DTO without purpose-specific naming

Reason:

    Inconsistent naming makes the project harder to understand, document, and review.

---

## Current Naming Baseline

The current accepted naming baseline is:

    Domain / Learning Project:
    MarketInsight Operations Tracker

    Repository:
    marketinsight-operations-tracker-api

    Solution:
    MarketInsight.OperationsTracker

    API Project:
    MarketInsight.Api

    Root Namespace:
    MarketInsight.Api

    Main API Resource:
    WatchlistItem

    Main Watchlist Route:
    /api/watchlist-items

---

## README Usage Standard

The README should use the repository display name:

    MarketInsight Operations Tracker API

The README should clearly state that this repository contains only the backend API component.

Recommended wording:

    This repository contains the backend API component of the MarketInsight Operations Tracker learning project.

---

## Documentation Usage Standard

When documentation refers to the overall domain, use:

    MarketInsight Operations Tracker

When documentation refers to this repository, use:

    MarketInsight Operations Tracker API

When documentation refers to the .NET solution, use:

    MarketInsight.OperationsTracker

When documentation refers to the Web API project, use:

    MarketInsight.Api

---

## Future Naming Extension

If the project later adds more components, naming can expand consistently.

Possible future examples:

    marketinsight-operations-tracker-web
    MarketInsight.Web

    marketinsight-operations-tracker-worker
    MarketInsight.Worker

    MarketInsight.Api.Tests

These are outside the current MVP scope.

---

## Review Checklist

Before adding new files, classes, routes, or documentation, check:

- Is the name consistent with the project naming standard?
- Does the name clearly describe the responsibility?
- Is the repository scope clear?
- Is the API project name used correctly?
- Are C# classes using PascalCase?
- Are route segments using kebab-case?
- Are documentation files using kebab-case?
- Does the route align with the domain concept?
- Does the DTO name describe request or response purpose?
- Does the controller name match the resource it manages?

---

## Summary

The project uses the following naming standard:

- Domain / learning project name: `MarketInsight Operations Tracker`
- Repository display name: `MarketInsight Operations Tracker API`
- Repository name: `marketinsight-operations-tracker-api`
- Solution name: `MarketInsight.OperationsTracker`
- API project name: `MarketInsight.Api`
- Root namespace: `MarketInsight.Api`

Core rule:

    Use names that clearly describe scope, responsibility, and domain meaning.

This standard should be followed before implementing Repository Pattern, LINQ queries, and Watchlist CRUD endpoints.