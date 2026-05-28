# API Route Naming Standard

This document defines the API route naming standard for the MarketInsight Operations Tracker API.

The purpose of this standard is to keep endpoint names clear, REST-oriented, consistent with domain concepts, and easy to understand before implementing Watchlist CRUD endpoints.

---

## Purpose

API routes should clearly describe the resource being accessed or changed.

In this project, route names should:

- Represent domain resources clearly.
- Use consistent naming.
- Avoid vague endpoint names.
- Follow REST-style resource naming.
- Stay understandable for a beginner backend project.
- Support future extension without unnecessary refactoring.

This standard is created before implementing Watchlist CRUD endpoints.

---

## Route Naming Principle

The project uses resource-based API route naming.

This means API routes should describe resources, not actions.

Correct mindset:

    GET /api/watchlist-items

Meaning:

    Get watchlist item resources.

Incorrect mindset:

    GET /api/get-watchlist

Problem:

    The route describes an action instead of a resource.

---

## Resource-Based Naming

Routes should be based on nouns.

Preferred:

    /api/watchlist-items

Avoid:

    /api/get-watchlist-items
    /api/create-watchlist-item
    /api/delete-watchlist-item

HTTP methods already describe the action.

The route should describe the resource.

Example:

| HTTP Method | Route | Meaning |
|---|---|---|
| GET | `/api/watchlist-items` | Get watchlist items |
| POST | `/api/watchlist-items` | Create a watchlist item |
| GET | `/api/watchlist-items/{symbol}` | Get one watchlist item by symbol |
| DELETE | `/api/watchlist-items/{symbol}` | Delete or deactivate one watchlist item |

---

## Why Not `/api/watchlist`

The earlier draft route was:

    /api/watchlist

This route is understandable, but it can be ambiguous.

It may mean:

- The watchlist itself
- The items inside the watchlist
- A future collection of multiple watchlists

In the current project scope, the system does not manage multiple watchlists.

Instead, it manages financial symbols tracked inside one logical watchlist.

The actual domain resource is:

    WatchlistItem

Therefore, the route should focus on watchlist items:

    /api/watchlist-items

---

## Current Watchlist Assumption

Current project assumption:

    The system has one logical watchlist.

The API manages the items inside that watchlist.

Therefore, the selected resource route is:

    /api/watchlist-items

This route is clearer because it directly maps to the domain entity:

    WatchlistItem

---

## Future Multi-Watchlist Scenario

If the system later supports multiple watchlists, the route standard may change.

Future possible route:

    /api/watchlists/{watchlistId}/items

This would mean:

    Get items inside a specific watchlist.

However, this is not part of the current MVP scope.

Current MVP route:

    /api/watchlist-items

Future multi-watchlist route:

    /api/watchlists/{watchlistId}/items

---

## Watchlist CRUD Route Standard

The Watchlist CRUD endpoints should use the following route standard.

| Operation | HTTP Method | API Route | Controller Method | Domain Concept |
|---|---|---|---|---|
| List active watchlist items | GET | `/api/watchlist-items` | `GetWatchlistItemsAsync` | `WatchlistItem` |
| Get watchlist item by symbol | GET | `/api/watchlist-items/{symbol}` | `GetWatchlistItemBySymbolAsync` | `WatchlistItem` |
| Create watchlist item | POST | `/api/watchlist-items` | `CreateWatchlistItemAsync` | `WatchlistItem` |
| Delete or deactivate watchlist item | DELETE | `/api/watchlist-items/{symbol}` | `DeleteWatchlistItemAsync` | `WatchlistItem` |

---

## Controller Naming Standard

Controller names should represent the resource they manage.

For Watchlist CRUD, the controller should be named:

    WatchlistItemsController

Reason:

    The controller manages WatchlistItem resources.

Avoid:

    WatchlistController

Reason:

    It may imply that the controller manages the watchlist container itself.

---

## Controller Method Naming Standard

Controller method names should be clear and asynchronous.

Recommended method names:

    GetWatchlistItemsAsync
    GetWatchlistItemBySymbolAsync
    CreateWatchlistItemAsync
    DeleteWatchlistItemAsync

Rules:

- Use clear domain names.
- Use `Async` suffix for asynchronous methods.
- Avoid vague names such as `Get`, `Post`, or `Delete`.
- Avoid action-style route names.
- Keep route behavior easy to read in Swagger and code review.

---

## Route Parameter Standard

Route parameters should represent stable lookup values.

For Watchlist CRUD, the selected route parameter is:

    symbol

Example:

    GET /api/watchlist-items/AAPL
    DELETE /api/watchlist-items/AAPL

The system should normalize symbol values internally.

For example:

    aapl
    Aapl
    AAPL

should be treated consistently as:

    AAPL

The route can accept user input in different casing, but the application should normalize it before querying or persisting data.

---

## Symbol Route Parameter

The `symbol` route parameter represents a financial symbol.

Examples:

    AAPL
    MSFT
    TSLA
    BTC

Route example:

    GET /api/watchlist-items/{symbol}

Concrete example:

    GET /api/watchlist-items/AAPL

Expected meaning:

    Get the watchlist item for AAPL.

---

## HTTP Method Responsibility

HTTP methods define the action.

The route defines the resource.

| HTTP Method | Responsibility |
|---|---|
| GET | Read resource data |
| POST | Create a new resource |
| PUT | Replace a resource |
| PATCH | Partially update a resource |
| DELETE | Delete or deactivate a resource |

For the current Watchlist CRUD scope:

| HTTP Method | Used For |
|---|---|
| GET | Listing or reading watchlist items |
| POST | Creating a watchlist item |
| DELETE | Deleting or deactivating a watchlist item |

---

## Soft Delete Route Behavior

The route uses DELETE:

    DELETE /api/watchlist-items/{symbol}

In the implementation, the system may perform soft delete instead of physical delete.

This means:

    The API behavior is delete/deactivate.
    The database record may remain with IsActive = false.

The route does not need to expose the internal soft delete detail.

Correct API perspective:

    DELETE /api/watchlist-items/AAPL

Internal implementation perspective:

    Set IsActive = false

---

## Route Naming Rules

The project should follow these route naming rules:

| Rule | Standard |
|---|---|
| Use `/api` prefix | Yes |
| Use resource nouns | Yes |
| Use plural resource names | Yes |
| Use kebab-case for multi-word route segments | Yes |
| Avoid verbs in route names | Yes |
| Keep route names domain-aligned | Yes |
| Keep future extensibility in mind | Yes |

---

## Kebab-Case Route Segments

Multi-word route segments should use kebab-case.

Preferred:

    /api/watchlist-items
    /api/price-snapshots
    /api/price-alerts
    /api/action-items

Avoid:

    /api/watchlistItems
    /api/watchlist_items
    /api/WatchlistItems

Reason:

    Kebab-case is readable and common in URL path design.

---

## Plural Resource Names

Collection routes should use plural resource names.

Preferred:

    /api/watchlist-items
    /api/price-snapshots
    /api/price-alerts
    /api/action-items

Avoid:

    /api/watchlist-item
    /api/price-snapshot
    /api/price-alert
    /api/action-item

Reason:

    Collection endpoints represent a set of resources.

---

## Route and Domain Mapping

Each API route should map clearly to a domain concept.

| Domain Concept | Route Segment |
|---|---|
| `WatchlistItem` | `watchlist-items` |
| `PriceSnapshot` | `price-snapshots` |
| `PriceAlert` | `price-alerts` |
| `ActionItem` | `action-items` |

This makes the API easier to understand because route names match business concepts.

---

## Planned Route Standards

The following routes are planned for the MVP scope.

### Health and System

| HTTP Method | Route | Purpose |
|---|---|---|
| GET | `/api/health` | Check basic API health |
| GET | `/api/system/info` | Return basic system information |
| GET | `/api/system/dependencies` | Return dependency status |

---

### Watchlist Items

| HTTP Method | Route | Purpose |
|---|---|---|
| GET | `/api/watchlist-items` | List active watchlist items |
| GET | `/api/watchlist-items/{symbol}` | Get watchlist item by symbol |
| POST | `/api/watchlist-items` | Create a new watchlist item |
| DELETE | `/api/watchlist-items/{symbol}` | Delete or deactivate a watchlist item |

---

### Price Snapshots

| HTTP Method | Route | Purpose |
|---|---|---|
| POST | `/api/watchlist-items/{symbol}/refresh` | Refresh price data synchronously |
| POST | `/api/watchlist-items/{symbol}/refresh-async` | Queue async price refresh |
| GET | `/api/watchlist-items/{symbol}/snapshots` | List price snapshots for a symbol |

---

### Price Alerts

| HTTP Method | Route | Purpose |
|---|---|---|
| POST | `/api/watchlist-items/{symbol}/alerts` | Create a price alert for a symbol |
| GET | `/api/watchlist-items/{symbol}/alerts` | List price alerts for a symbol |
| DELETE | `/api/price-alerts/{id}` | Delete or deactivate a price alert |

---

### Action Items

| HTTP Method | Route | Purpose |
|---|---|---|
| GET | `/api/action-items` | List action items |
| PATCH | `/api/action-items/{id}/complete` | Mark an action item as completed |

---

### Cache

| HTTP Method | Route | Purpose |
|---|---|---|
| GET | `/api/cache/quotes/{symbol}` | Get cached quote for a symbol |
| DELETE | `/api/cache/quotes/{symbol}` | Remove cached quote for a symbol |

---

## Route Design Notes

### Nested Routes

Nested routes may be used when a resource clearly belongs to another resource.

Example:

    GET /api/watchlist-items/{symbol}/snapshots

Meaning:

    Get snapshots for a specific watchlist item.

Example:

    GET /api/watchlist-items/{symbol}/alerts

Meaning:

    Get alerts for a specific watchlist item.

---

### Non-Nested Routes

Non-nested routes may be used when a resource can be managed independently.

Example:

    DELETE /api/price-alerts/{id}

Reason:

    A price alert has its own unique identifier.

Example:

    PATCH /api/action-items/{id}/complete

Reason:

    An action item is managed by its own ID.

---

## Route Naming Anti-Patterns

Avoid action-based route names.

Do not use:

    /api/get-watchlist-items
    /api/create-watchlist-item
    /api/delete-watchlist-item
    /api/update-watchlist-item

Use:

    GET /api/watchlist-items
    POST /api/watchlist-items
    DELETE /api/watchlist-items/{symbol}

Reason:

    HTTP method already describes the action.

---

## Controller Route Attribute Standard

For Watchlist CRUD, the controller route should be:

    [Route("api/watchlist-items")]

The controller name should be:

    WatchlistItemsController

Example endpoint mapping:

| Controller Method | HTTP Attribute | Route Result |
|---|---|---|
| `GetWatchlistItemsAsync` | `[HttpGet]` | `GET /api/watchlist-items` |
| `GetWatchlistItemBySymbolAsync` | `[HttpGet("{symbol}")]` | `GET /api/watchlist-items/{symbol}` |
| `CreateWatchlistItemAsync` | `[HttpPost]` | `POST /api/watchlist-items` |
| `DeleteWatchlistItemAsync` | `[HttpDelete("{symbol}")]` | `DELETE /api/watchlist-items/{symbol}` |

---

## Swagger Documentation Impact

Swagger should display clear route names.

The selected route standard improves Swagger readability.

Example Swagger groups:

    GET /api/watchlist-items
    POST /api/watchlist-items
    GET /api/watchlist-items/{symbol}
    DELETE /api/watchlist-items/{symbol}

This makes the API contract easier to test and explain.

---

## API Contract Impact

Changing from:

    /api/watchlist

to:

    /api/watchlist-items

means API documentation and future endpoint implementation should follow the new route.

Documents that should be aligned:

- `docs/api-contracts/api-endpoint-draft.md`
- `README.md`
- Future controller XML Summary comments
- Future Swagger endpoint descriptions

---

## Current Decision

For the current MVP scope, the selected route standard is:

    /api/watchlist-items

Reason:

- The system manages watchlist item resources.
- The system does not currently manage multiple watchlists.
- The route maps clearly to the `WatchlistItem` domain concept.
- The route is less ambiguous than `/api/watchlist`.
- It is suitable before implementing Watchlist CRUD.

---

## Future Decision Point

If the project later supports multiple watchlists, a future route standard may be introduced:

    /api/watchlists/{watchlistId}/items

This is outside the current MVP scope.

The current MVP will use:

    /api/watchlist-items

---

## Review Checklist

Before implementing a new endpoint, check:

- Does the route represent a resource?
- Is the route noun-based?
- Is the collection route plural?
- Is kebab-case used for multi-word route segments?
- Is the route aligned with a domain concept?
- Does the HTTP method describe the action?
- Is business behavior not encoded as a route verb?
- Is the controller method name clear and asynchronous?
- Is Swagger output easy to understand?

---

## Summary

The project uses resource-based API route naming.

Core rules:

- Use `/api` prefix.
- Use nouns instead of verbs.
- Use plural resource names for collections.
- Use kebab-case for multi-word route segments.
- Keep routes aligned with domain concepts.
- Use HTTP methods to describe actions.
- Use `/api/watchlist-items` for Watchlist CRUD in the current MVP.

Selected Watchlist CRUD routes:

    GET    /api/watchlist-items
    GET    /api/watchlist-items/{symbol}
    POST   /api/watchlist-items
    DELETE /api/watchlist-items/{symbol}