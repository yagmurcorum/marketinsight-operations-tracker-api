# Entity Relationship Model

This document explains the entity relationship model of the MarketInsight Operations Tracker API.

The purpose of this document is to clarify how the main database entities relate to each other before implementing Repository Pattern, LINQ queries, and Watchlist CRUD endpoints.

---

## Purpose

The project uses a small but realistic financial tracking domain.

The database model is designed around the following concepts:

- A financial symbol can be added to a watchlist.
- A tracked symbol can have price snapshot records.
- A tracked symbol can have price alert rules.
- A price alert or important market event can create an operational action item.

This document defines:

- Which entity is the main entity.
- Which entities depend on another entity.
- Which entity is created by which operation.
- How relationships should be understood before implementing data access and CRUD behavior.

---

## Entity Overview

| Entity | Role | Description |
|---|---|---|
| `WatchlistItem` | Main entity | Represents a financial symbol tracked by the system |
| `PriceSnapshot` | Dependent entity | Represents a saved price record for a tracked symbol |
| `PriceAlert` | Dependent entity | Represents a price rule defined for a tracked symbol |
| `ActionItem` | Dependent operational entity | Represents a follow-up task created after an alert or important market event |

---

## Main Entity

The main entity in the current domain model is:

    WatchlistItem

Reason:

    All price tracking, alerting, and operational follow-up behavior starts from a tracked financial symbol.

A `WatchlistItem` represents the symbol that the system follows.

Examples:

    AAPL
    MSFT
    TSLA
    BTC

The other entities are connected to this tracked symbol.

---

## High-Level Relationship Diagram

    WatchlistItem
        ├── PriceSnapshot
        ├── PriceAlert
        └── ActionItem

    PriceAlert
        └── ActionItem

Meaning:

- A `WatchlistItem` can have many `PriceSnapshot` records.
- A `WatchlistItem` can have many `PriceAlert` records.
- A `WatchlistItem` can have many `ActionItem` records.
- A `PriceAlert` can optionally be connected to one or more `ActionItem` records.

---

## Relationship Summary

| Relationship | Type | Meaning |
|---|---|---|
| `WatchlistItem` → `PriceSnapshot` | One-to-many | One tracked symbol can have many price snapshot records |
| `WatchlistItem` → `PriceAlert` | One-to-many | One tracked symbol can have many price alert rules |
| `WatchlistItem` → `ActionItem` | One-to-many | One tracked symbol can have many operational follow-up actions |
| `PriceAlert` → `ActionItem` | Optional one-to-many | One price alert can create many action items over time |

---

## WatchlistItem

### Purpose

`WatchlistItem` represents a financial symbol tracked by the system.

It is the main entity of the current MVP domain.

Example:

    AAPL is added to the watchlist.

This means the system now has a `WatchlistItem` record for AAPL.

---

### Main Responsibilities

`WatchlistItem` is responsible for representing:

- The tracked symbol
- The normalized symbol value
- Optional display name
- Optional market information
- Active/passive status
- Creation and update timestamps

---

### Example Data

| Field | Example |
|---|---|
| `Id` | `1` |
| `Symbol` | `AAPL` |
| `NormalizedSymbol` | `AAPL` |
| `DisplayName` | `Apple Inc.` |
| `Market` | `NASDAQ` |
| `IsActive` | `true` |
| `CreatedAtUtc` | `2026-05-26T10:00:00Z` |

---

### Created By

A `WatchlistItem` is created when the user adds a new symbol to the watchlist.

Future endpoint:

    POST /api/watchlist-items

---

### Used By

`WatchlistItem` will be used by:

- Watchlist CRUD endpoints
- Repository Pattern
- EF Core queries
- Price refresh flow
- Price snapshot creation
- Price alert creation
- Action item creation

---

## PriceSnapshot

### Purpose

`PriceSnapshot` represents a saved price record for a tracked symbol.

It is used to keep historical price data.

Example:

    AAPL price was 180.25 USD at a specific time.

This is a price snapshot.

---

### Relationship With WatchlistItem

Relationship:

    WatchlistItem 1 → N PriceSnapshot

Meaning:

    One WatchlistItem can have many PriceSnapshot records.

Example:

    WatchlistItem: AAPL

    PriceSnapshot 1: AAPL - 180.25 USD - 10:00 UTC
    PriceSnapshot 2: AAPL - 181.10 USD - 11:00 UTC
    PriceSnapshot 3: AAPL - 179.90 USD - 12:00 UTC

All three snapshots belong to the same `WatchlistItem`.

---

### Foreign Key

`PriceSnapshot` should contain:

    WatchlistItemId

This connects the price snapshot to its parent `WatchlistItem`.

---

### Created By

A `PriceSnapshot` is created when price data is retrieved and persisted.

Future operations:

    POST /api/watchlist-items/{symbol}/refresh
    POST /api/watchlist-items/{symbol}/refresh-async

---

### Used By

`PriceSnapshot` will be used by:

- Price refresh flow
- Historical price tracking
- Snapshot listing endpoint
- Future analytics or observability scenarios

Future endpoint:

    GET /api/watchlist-items/{symbol}/snapshots

---

## PriceAlert

### Purpose

`PriceAlert` represents a price rule for a tracked symbol.

It does not store price history.

It stores a condition that should be evaluated against price data.

Example:

    Notify or create an action when AAPL goes above 200 USD.

---

### Relationship With WatchlistItem

Relationship:

    WatchlistItem 1 → N PriceAlert

Meaning:

    One WatchlistItem can have many PriceAlert records.

Example:

    WatchlistItem: AAPL

    PriceAlert 1: AAPL above 200 USD
    PriceAlert 2: AAPL below 170 USD

Both alerts belong to the same `WatchlistItem`.

---

### Foreign Key

`PriceAlert` should contain:

    WatchlistItemId

This connects the price alert to its parent `WatchlistItem`.

---

### Created By

A `PriceAlert` is created when the user defines an alert rule for a tracked symbol.

Future endpoint:

    POST /api/watchlist-items/{symbol}/alerts

---

### Used By

`PriceAlert` will be used by:

- Alert creation endpoint
- Alert listing endpoint
- Alert evaluation logic
- Future action item creation flow

Future endpoint:

    GET /api/watchlist-items/{symbol}/alerts

---

## ActionItem

### Purpose

`ActionItem` represents an operational follow-up task.

It is created when the system identifies something that should be reviewed or completed.

Example:

    AAPL crossed above 200 USD. Review this symbol.

---

### Relationship With WatchlistItem

Relationship:

    WatchlistItem 1 → N ActionItem

Meaning:

    One WatchlistItem can have many ActionItem records.

Example:

    WatchlistItem: AAPL

    ActionItem 1: Review AAPL because alert was triggered.
    ActionItem 2: Check AAPL after important price movement.

Both action items belong to the same `WatchlistItem`.

---

### Relationship With PriceAlert

Relationship:

    PriceAlert 1 → N ActionItem

This relationship is optional from the `ActionItem` side.

Meaning:

    An ActionItem may be created because of a PriceAlert.
    An ActionItem may also be created because of another important market event in the future.

Therefore:

    PriceAlertId can be nullable.

---

### Foreign Keys

`ActionItem` should contain:

    WatchlistItemId
    PriceAlertId

`WatchlistItemId` is required.

`PriceAlertId` can be optional.

Reason:

    Every ActionItem should belong to a tracked symbol.
    Not every ActionItem must come from a PriceAlert.

---

### Created By

An `ActionItem` can be created when:

- A price alert condition is triggered.
- A future important price event is detected.
- A future operational workflow requires a follow-up task.

Future endpoints:

    GET /api/action-items
    PATCH /api/action-items/{id}/complete

---

## Relationship Details

## WatchlistItem and PriceSnapshot

### Relationship

    One WatchlistItem can have many PriceSnapshots.

### Why This Exists

A tracked symbol can have price records collected at different times.

Example:

    AAPL at 10:00
    AAPL at 11:00
    AAPL at 12:00

Each record is a separate `PriceSnapshot`.

---

### Business Meaning

This relationship allows the system to answer:

    What was the price of this symbol over time?

---

### Technical Meaning

`PriceSnapshot` should store the parent `WatchlistItemId`.

This allows EF Core and Repository queries to retrieve snapshots for a specific symbol.

---

## WatchlistItem and PriceAlert

### Relationship

    One WatchlistItem can have many PriceAlerts.

### Why This Exists

A tracked symbol can have more than one alert rule.

Example:

    AAPL above 200 USD
    AAPL below 170 USD

Both rules are connected to the same tracked symbol.

---

### Business Meaning

This relationship allows the system to answer:

    Which alert rules are defined for this symbol?

---

### Technical Meaning

`PriceAlert` should store the parent `WatchlistItemId`.

This allows EF Core and Repository queries to retrieve alerts for a specific symbol.

---

## WatchlistItem and ActionItem

### Relationship

    One WatchlistItem can have many ActionItems.

### Why This Exists

A tracked symbol can generate multiple follow-up tasks over time.

Example:

    Review AAPL after alert trigger.
    Check AAPL after unusual movement.
    Complete AAPL follow-up task.

---

### Business Meaning

This relationship allows the system to answer:

    Which operational actions are related to this tracked symbol?

---

### Technical Meaning

`ActionItem` should store the parent `WatchlistItemId`.

This keeps every action item tied to a tracked symbol.

---

## PriceAlert and ActionItem

### Relationship

    One PriceAlert can have many ActionItems.

From the `ActionItem` side, this relationship is optional.

### Why This Exists

A price alert can trigger follow-up actions.

Example:

    PriceAlert: AAPL above 200 USD
    ActionItem: Review AAPL because the alert was triggered.

---

### Business Meaning

This relationship allows the system to answer:

    Which action items were created because of this price alert?

---

### Technical Meaning

`ActionItem` may store:

    PriceAlertId

This field can be nullable because some action items may not come from a price alert.

---

## Entity Creation Flow

## WatchlistItem Creation

Future endpoint:

    POST /api/watchlist-items

Flow:

    CreateWatchlistItemRequest DTO
          ↓
    Validate input
          ↓
    Normalize symbol
          ↓
    Check unique symbol rule
          ↓
    Create WatchlistItem entity
          ↓
    Save to SQLite database

Result:

    New WatchlistItem record

---

## PriceSnapshot Creation

Future endpoint:

    POST /api/watchlist-items/{symbol}/refresh

Flow:

    Find WatchlistItem by symbol
          ↓
    Retrieve price from external finance API
          ↓
    Create PriceSnapshot entity
          ↓
    Save to SQLite database

Result:

    New PriceSnapshot record linked to WatchlistItem

---

## PriceAlert Creation

Future endpoint:

    POST /api/watchlist-items/{symbol}/alerts

Flow:

    Find WatchlistItem by symbol
          ↓
    Validate alert condition
          ↓
    Create PriceAlert entity
          ↓
    Save to SQLite database

Result:

    New PriceAlert record linked to WatchlistItem

---

## ActionItem Creation

Future trigger:

    PriceAlert condition is met

Flow:

    New price data is retrieved
          ↓
    Active PriceAlerts are evaluated
          ↓
    A PriceAlert condition is met
          ↓
    Create ActionItem entity
          ↓
    Link ActionItem to WatchlistItem
          ↓
    Optionally link ActionItem to PriceAlert
          ↓
    Save to SQLite database

Result:

    New ActionItem record

---

## Entity Relationship Table

| Parent Entity | Child Entity | Relationship | Required Foreign Key | Optional Foreign Key | Created When |
|---|---|---|---|---|---|
| `WatchlistItem` | `PriceSnapshot` | One-to-many | `WatchlistItemId` | None | Price refresh stores price data |
| `WatchlistItem` | `PriceAlert` | One-to-many | `WatchlistItemId` | None | User creates an alert rule |
| `WatchlistItem` | `ActionItem` | One-to-many | `WatchlistItemId` | None | Operational follow-up is created |
| `PriceAlert` | `ActionItem` | Optional one-to-many | None | `PriceAlertId` | Alert condition creates an action item |

---

## Database Responsibility

The database should preserve relationships between records.

This means:

- `WatchlistItem` is the parent record for symbol-based data.
- `PriceSnapshot` should not exist without a related `WatchlistItem`.
- `PriceAlert` should not exist without a related `WatchlistItem`.
- `ActionItem` should not exist without a related `WatchlistItem`.
- `ActionItem` may or may not be related to a `PriceAlert`.

---

## API Responsibility

The API should not expose relationship complexity directly unless needed.

For example:

    GET /api/watchlist-items

should return clean watchlist item response DTOs.

It should not automatically return all snapshots, alerts, and action items unless a specific endpoint requires that data.

Specific related data should be accessed through specific endpoints.

Examples:

    GET /api/watchlist-items/{symbol}/snapshots
    GET /api/watchlist-items/{symbol}/alerts
    GET /api/action-items

---

## DTO Responsibility

DTOs should shape API input and output.

DTOs should not expose full entity relationship graphs by default.

Reason:

- Keeps API responses clean.
- Avoids unnecessary data loading.
- Avoids circular reference risks.
- Keeps API contract stable.
- Separates database structure from public response shape.

---

## Repository Responsibility

Repositories should use these relationships for database queries.

Example future repository query responsibilities:

- Get active watchlist items.
- Get a watchlist item by normalized symbol.
- Get snapshots by watchlist item.
- Get alerts by watchlist item.
- Get action items by watchlist item.
- Add new child records to the correct parent entity.

---

## Current MVP Relationship Scope

The current MVP uses a simple relationship model.

Included:

- One logical watchlist
- Many watchlist items
- Price snapshots connected to watchlist items
- Price alerts connected to watchlist items
- Action items connected to watchlist items
- Optional action item connection to price alerts

Not included:

- Multiple user accounts
- Multiple user-specific watchlists
- Portfolio management
- Trading execution
- Real-time streaming market data
- Complex alert rule engine
- Multi-tenant data ownership

---

## Future Extension Notes

If multiple watchlists are introduced in the future, the model may need a new parent entity:

    Watchlist

Possible future relationship:

    Watchlist
        └── WatchlistItem

Possible future route:

    /api/watchlists/{watchlistId}/items

This is outside the current MVP scope.

The current MVP keeps the model simple:

    WatchlistItem is the main tracked symbol entity.

---

## Review Checklist

Before implementing Repository Pattern or CRUD endpoints, check:

- Is `WatchlistItem` treated as the main entity?
- Are `PriceSnapshot` records linked to `WatchlistItem`?
- Are `PriceAlert` records linked to `WatchlistItem`?
- Are `ActionItem` records linked to `WatchlistItem`?
- Is `PriceAlertId` optional on `ActionItem`?
- Are Entity relationships kept inside the persistence model?
- Are DTOs used instead of exposing full entity graphs?
- Are future endpoints aligned with these relationships?
- Are database relationships easy to explain?

---

## Summary

The entity relationship model is centered around `WatchlistItem`.

Core relationship rules:

- `WatchlistItem` is the main tracked symbol entity.
- One `WatchlistItem` can have many `PriceSnapshot` records.
- One `WatchlistItem` can have many `PriceAlert` records.
- One `WatchlistItem` can have many `ActionItem` records.
- One `PriceAlert` can optionally create many `ActionItem` records.
- `ActionItem` must belong to a `WatchlistItem`.
- `ActionItem` may optionally belong to a `PriceAlert`.

Main model:

    WatchlistItem
        ├── PriceSnapshot
        ├── PriceAlert
        └── ActionItem

    PriceAlert
        └── ActionItem