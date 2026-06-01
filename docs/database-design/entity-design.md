# Database Entity Design and Initial DTO Models

## Purpose

This document describes the database entity and DTO model structure used in the MarketInsight Operations Tracker API.

The purpose of this document is to define the data model foundation for SQLite, EF Core, DbContext, migration, Repository Pattern, Watchlist CRUD endpoints, soft delete behavior, and reactivation behavior.

This document covers:

    WatchlistItem
    PriceSnapshot
    PriceAlert
    ActionItem
    CreateWatchlistItemRequest
    WatchlistItemResponse

---

## Week 2 Context

Week 2 focuses on the data model foundation of the project.

The main topics of this week are:

- Entity design
- DTO design
- SQLite
- EF Core
- DbContext
- Migration
- Repository Pattern
- Watchlist CRUD operations
- Soft delete behavior
- Reactivation behavior

Before creating database-backed API endpoints, the project needs clear entity and DTO models.

---

## Entity and DTO Separation

The project separates Entity models and DTO models.

Entity models represent database and persistence structure.

DTO models represent API request and response contracts.

This separation prevents database models from being directly exposed through the public API.

Basic rule:

    Entity = database / persistence model
    DTO = API request / response model

Entity models are located in:

    src/MarketInsight.Api/Entities

DTO models are located in:

    src/MarketInsight.Api/DTOs

Watchlist-related DTO models are located in:

    src/MarketInsight.Api/DTOs/Watchlist

---

## Implemented Models

| Model | Type | Purpose |
|---|---|---|
| `WatchlistItem` | Entity | Represents a financial symbol tracked by the user |
| `PriceSnapshot` | Entity | Represents a persistent price history record for a tracked financial symbol |
| `PriceAlert` | Entity | Represents a price alert rule defined for a tracked financial symbol |
| `ActionItem` | Entity | Represents an operational follow-up action created after a price alert or important price event |
| `CreateWatchlistItemRequest` | DTO | Represents the request body used to create or reactivate a financial symbol in the watchlist |
| `WatchlistItemResponse` | DTO | Represents the response model returned for a watchlist item |

---

## WatchlistItem Entity

`WatchlistItem` represents a financial symbol that the user wants to track.

Example tracked symbols:

    AAPL
    TSLA
    MSFT
    BTC

The `WatchlistItem` entity is the main entity of the Watchlist module.

It is used for:

- Adding a symbol to the watchlist
- Listing tracked symbols
- Getting symbol details
- Removing or deactivating a symbol from the watchlist
- Reactivating a previously deleted symbol
- Connecting price snapshots to a tracked symbol
- Connecting price alerts to a tracked symbol
- Connecting action items to a tracked symbol

Implementation location:

    src/MarketInsight.Api/Entities/WatchlistItem.cs

### WatchlistItem Properties

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Unique identifier of the watchlist item |
| `Symbol` | `string` | Display financial symbol stored by the system |
| `NormalizedSymbol` | `string` | Normalized symbol used for lookup, comparison, duplicate checks, and reactivation checks |
| `DisplayName` | `string?` | Optional display name for the financial symbol |
| `Market` | `string?` | Optional market information for the financial symbol |
| `IsActive` | `bool` | Indicates whether the symbol is active in the watchlist |
| `CreatedAtUtc` | `DateTime` | UTC date and time when the watchlist item was created |
| `UpdatedAtUtc` | `DateTime?` | UTC date and time when the watchlist item was last updated, deleted, or reactivated |

---

## WatchlistItem Lifecycle

The project uses one database row per normalized symbol.

This means:

    AAPL should exist once as a WatchlistItem row.

The row can move between active and inactive states.

Lifecycle:

    New symbol is created
      ↓
    IsActive = true
      ↓
    Symbol is deleted
      ↓
    IsActive = false
      ↓
    Same symbol is posted again
      ↓
    Existing row is reactivated
      ↓
    IsActive = true

This avoids duplicate rows while still allowing a previously deleted symbol to be added again.

---

## WatchlistItem Create Behavior

The WatchlistItem create flow uses `NormalizedSymbol` to decide whether to create, reject, or reactivate a record.

Current behavior:

| Existing Record State | API Behavior | Status Code |
|---|---|---|
| No existing record with same `NormalizedSymbol` | Create new `WatchlistItem` | `201 Created` |
| Existing record is active | Return duplicate conflict | `409 Conflict` |
| Existing record is inactive | Reactivate existing `WatchlistItem` | `200 OK` |

This behavior keeps the database consistent and avoids confusing API behavior.

---

## WatchlistItem Usage Flow

The create or reactivation flow is:

    Client / Swagger
      ↓
    POST /api/watchlist-items
      ↓
    CreateWatchlistItemRequest
      ↓
    Service normalizes Symbol
      ↓
    Repository checks existing WatchlistItem by NormalizedSymbol
      ↓
    Service decides create, duplicate, or reactivation
      ↓
    WatchlistItem
      ↓
    SQLite database
      ↓
    WatchlistItemResponse

The list flow is:

    Client / Swagger
      ↓
    GET /api/watchlist-items
      ↓
    Active WatchlistItem records
      ↓
    WatchlistItemResponse
      ↓
    200 OK Response

The delete flow is:

    Client / Swagger
      ↓
    DELETE /api/watchlist-items/{symbol}
      ↓
    Service normalizes Symbol
      ↓
    Repository finds WatchlistItem by NormalizedSymbol
      ↓
    Service sets IsActive = false
      ↓
    Service sets UpdatedAtUtc = DateTime.UtcNow
      ↓
    SQLite database
      ↓
    204 No Content Response

---

## Soft Delete Behavior

The project uses soft delete for WatchlistItem records.

Soft delete means the row is not physically removed from the database.

Instead, the system updates:

    IsActive = false
    UpdatedAtUtc = current UTC time

After soft delete:

- The item remains in the database.
- The item does not appear in `GET /api/watchlist-items`.
- The item does not return as active from `GET /api/watchlist-items/{symbol}`.
- The item can be reactivated by posting the same symbol again.

This keeps historical continuity for the symbol while keeping active API responses clean.

---

## Reactivation Behavior

If a user posts a symbol that already exists but is inactive, the application reactivates the existing row.

Reactivation updates:

    IsActive = true
    UpdatedAtUtc = current UTC time

Reactivation may also update:

    DisplayName
    Market

Reactivation does not change:

    Id
    CreatedAtUtc

Reactivation does not create:

    A new WatchlistItem row

Expected API response:

    200 OK

This behavior works together with the unique index on `NormalizedSymbol`.

---

## PriceSnapshot Entity

`PriceSnapshot` represents a persistent price history record for a tracked financial symbol.

A price snapshot is created when price data is successfully retrieved for a tracked financial symbol.

`PriceSnapshot` does not represent the tracked symbol itself. The tracked symbol is represented by `WatchlistItem`.

`PriceSnapshot` represents the price history of that symbol.

Example:

    AAPL - 180.25 USD - Finnhub - 2026-05-26 10:00 UTC

Implementation location:

    src/MarketInsight.Api/Entities/PriceSnapshot.cs

### PriceSnapshot Properties

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Unique identifier of the price snapshot |
| `WatchlistItemId` | `int` | Identifier of the related watchlist item |
| `Symbol` | `string` | Financial symbol related to this price snapshot |
| `Price` | `decimal` | Price value retrieved from the external finance API |
| `Currency` | `string` | Currency of the retrieved price value |
| `Source` | `string` | Source of the retrieved price data |
| `RetrievedAtUtc` | `DateTime` | UTC date and time when the price data was retrieved |
| `CreatedAtUtc` | `DateTime` | UTC date and time when the price snapshot record was created |
| `WatchlistItem` | `WatchlistItem?` | Related watchlist item navigation property |

### PriceSnapshot Usage Flow

The future price refresh flow will be:

    Client / Swagger
      ↓
    POST /api/watchlist-items/{symbol}/refresh
      ↓
    Check active WatchlistItem
      ↓
    Retrieve price from external finance API
      ↓
    Save PriceSnapshot
      ↓
    Return price refresh response

The future price history flow will be:

    Client / Swagger
      ↓
    GET /api/watchlist-items/{symbol}/snapshots
      ↓
    Read PriceSnapshot records
      ↓
    Return historical price data

### PriceSnapshot Responsibility

`PriceSnapshot` is responsible for storing historical price records.

It supports the following MVP requirement:

    SQLite / EF Core
    Persistent data and snapshot record
    PriceSnapshot persistence

This means the system should not only show the latest retrieved price. It should also keep historical price records in SQLite.

---

## PriceAlert Entity

`PriceAlert` represents a price alert rule defined for a tracked financial symbol.

A price alert is used when a specific price level becomes important.

Example conditions:

    AAPL price goes above 200
    TSLA price goes below 150

Implementation location:

    src/MarketInsight.Api/Entities/PriceAlert.cs

### PriceAlert Properties

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Unique identifier of the price alert |
| `WatchlistItemId` | `int` | Identifier of the related watchlist item |
| `Symbol` | `string` | Financial symbol related to this price alert |
| `ConditionType` | `string` | Alert condition type, such as Above or Below |
| `TargetPrice` | `decimal` | Target price value used when evaluating the alert condition |
| `IsActive` | `bool` | Indicates whether the price alert is active |
| `CreatedAtUtc` | `DateTime` | UTC date and time when the price alert was created |
| `LastTriggeredAtUtc` | `DateTime?` | UTC date and time when the price alert was last triggered |
| `WatchlistItem` | `WatchlistItem?` | Related watchlist item navigation property |

### PriceAlert Usage Flow

The future price alert flow will be:

    Client / Swagger
      ↓
    POST /api/watchlist-items/{symbol}/alerts
      ↓
    Check active WatchlistItem
      ↓
    Create price alert rule
      ↓
    Save PriceAlert

The future alert evaluation flow will be:

    Price refresh completed
      ↓
    New PriceSnapshot created
      ↓
    Active PriceAlert records are checked
      ↓
    Condition is evaluated
      ↓
    ActionItem may be created

### PriceAlert Responsibility

`PriceAlert` does not store historical prices.

Historical prices are stored by:

    PriceSnapshot

`PriceAlert` stores the condition that makes a price important.

Example conditions:

    Current price > TargetPrice
    Current price < TargetPrice

---

## ActionItem Entity

`ActionItem` represents an operational follow-up action created after a price alert or important price event.

An action item is created when the system needs to track follow-up work.

Example:

    AAPL price crossed above 200. Review this symbol.

Implementation location:

    src/MarketInsight.Api/Entities/ActionItem.cs

### ActionItem Properties

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Unique identifier of the action item |
| `WatchlistItemId` | `int` | Identifier of the related watchlist item |
| `PriceAlertId` | `int?` | Optional identifier of the related price alert |
| `Symbol` | `string` | Financial symbol related to this action item |
| `Title` | `string` | Short title of the action item |
| `Description` | `string?` | Optional detailed description of the action item |
| `Status` | `string` | Current status of the action item, such as Pending or Completed |
| `CreatedAtUtc` | `DateTime` | UTC date and time when the action item was created |
| `CompletedAtUtc` | `DateTime?` | UTC date and time when the action item was completed |
| `WatchlistItem` | `WatchlistItem?` | Related watchlist item navigation property |
| `PriceAlert` | `PriceAlert?` | Related price alert navigation property |

### ActionItem Usage Flow

The future action item flow will be:

    PriceAlert condition is met
      ↓
    Alert evaluation logic runs
      ↓
    ActionItem is created
      ↓
    User lists action items
      ↓
    User marks action item as completed

The future action endpoints will be:

    GET /api/action-items
    PATCH /api/action-items/{id}/complete

---

## Entity Relationships

Detailed relationship rules are documented in:

    docs/database-design/entity-relationship-model.md

This section gives a short relationship overview for the entity design document.

The entity relationship structure is:

    WatchlistItem
      ├── PriceSnapshot
      ├── PriceAlert
      └── ActionItem

    PriceAlert
      └── ActionItem

Relationship details:

| Relationship | Meaning |
|---|---|
| `WatchlistItem 1 - N PriceSnapshot` | One tracked symbol can have many price history records |
| `WatchlistItem 1 - N PriceAlert` | One tracked symbol can have many price alert rules |
| `WatchlistItem 1 - N ActionItem` | One tracked symbol can have many follow-up actions |
| `PriceAlert 1 - N ActionItem` | One price alert can generate many follow-up actions over time |

Example:

    WatchlistItem:
        AAPL

    PriceSnapshot records:
        AAPL - 180.25 USD - 10:00 UTC
        AAPL - 181.10 USD - 11:00 UTC

    PriceAlert:
        AAPL above 200

    Generated ActionItem:
        AAPL price crossed above 200. Review this symbol.

`PriceAlertId` is nullable in `ActionItem`.

This allows an action item to be created from a price alert or from another important price event in the future.

---

## Watchlist DTO Models

The first DTO models are focused on Watchlist operations.

Implemented DTOs:

    CreateWatchlistItemRequest
    WatchlistItemResponse

---

## CreateWatchlistItemRequest DTO

`CreateWatchlistItemRequest` represents the request body used when creating or reactivating a financial symbol in the watchlist.

Implementation location:

    src/MarketInsight.Api/DTOs/Watchlist/CreateWatchlistItemRequest.cs

Example request body:

    {
      "symbol": "AAPL",
      "displayName": "Apple Inc.",
      "market": "NASDAQ"
    }

### CreateWatchlistItemRequest Properties

| Property | Type | Validation | Purpose |
|---|---|---|---|
| `Symbol` | `string` | Required, maximum length 20 | Financial symbol requested by the user, such as AAPL or TSLA |
| `DisplayName` | `string?` | Maximum length 100 | Optional display name for the financial symbol |
| `Market` | `string?` | Maximum length 50 | Optional market information for the financial symbol |

---

## WatchlistItemResponse DTO

`WatchlistItemResponse` represents the response model returned when watchlist item data is exposed through the API.

Implementation location:

    src/MarketInsight.Api/DTOs/Watchlist/WatchlistItemResponse.cs

Example response body:

    {
      "id": 1,
      "symbol": "AAPL",
      "normalizedSymbol": "AAPL",
      "displayName": "Apple Inc.",
      "market": "NASDAQ",
      "isActive": true,
      "createdAtUtc": "2026-05-26T10:00:00Z",
      "updatedAtUtc": null
    }

### WatchlistItemResponse Properties

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Unique identifier of the watchlist item |
| `Symbol` | `string` | Financial symbol stored in the watchlist |
| `NormalizedSymbol` | `string` | Normalized financial symbol used by the system |
| `DisplayName` | `string?` | Optional display name for the financial symbol |
| `Market` | `string?` | Optional market information for the financial symbol |
| `IsActive` | `bool` | Indicates whether the symbol is active in the watchlist |
| `CreatedAtUtc` | `DateTime` | UTC date and time when the watchlist item was created |
| `UpdatedAtUtc` | `DateTime?` | UTC date and time when the watchlist item was last updated, deleted, or reactivated |

---

## API Model Flow

The Watchlist create or reactivation flow uses the models in this order:

    CreateWatchlistItemRequest
      ↓
    WatchlistItem
      ↓
    WatchlistItemResponse

This means:

    API request model
      ↓
    Database model
      ↓
    API response model

This keeps the API contract separate from the database structure.

---

## Current Scope

This document covers the entity and DTO model design.

The following items are included in the current scope:

- Entity models
- Watchlist DTO models
- Entity and DTO separation
- Basic validation attributes on request DTO
- Entity relationship overview
- WatchlistItem soft delete behavior
- WatchlistItem reactivation behavior
- PriceSnapshot persistence purpose
- Future PriceAlert and ActionItem purpose
- Build verification

---

## Out of Scope

The following topics are outside the scope of this document and are documented separately as they are implemented:

- EF Core package installation
- DbContext configuration
- SQLite connection string
- EF Core migration
- Repository Pattern implementation details
- Full Watchlist CRUD API contract
- External Finance API integration
- Redis cache
- RabbitMQ messaging
- Background Worker
- Price refresh service
- Alert evaluation logic
- ActionItem endpoint implementation

---

## Build Verification

After adding the initial entity and DTO models, the project build was executed.

Build command:

    dotnet build

Result:

    Build succeeded

This confirms that the entity and DTO models compile successfully.

---

## Related Files

| File | Purpose |
|---|---|
| `src/MarketInsight.Api/Entities/WatchlistItem.cs` | Main watchlist entity |
| `src/MarketInsight.Api/Entities/PriceSnapshot.cs` | Price history entity |
| `src/MarketInsight.Api/Entities/PriceAlert.cs` | Price alert rule entity |
| `src/MarketInsight.Api/Entities/ActionItem.cs` | Follow-up action entity |
| `src/MarketInsight.Api/DTOs/Watchlist/CreateWatchlistItemRequest.cs` | Watchlist create request DTO |
| `src/MarketInsight.Api/DTOs/Watchlist/WatchlistItemResponse.cs` | Watchlist response DTO |
| `docs/database-design/entity-design.md` | Documents the entity and DTO model design |
| `docs/database-design/entity-relationship-model.md` | Documents the detailed entity relationship rules |
| `docs/database-design/entity-constraint-standards.md` | Documents uniqueness, normalization, soft delete, and reactivation standards |
| `docs/api-contracts/watchlist-items-api-contract.md` | Documents Watchlist Items API behavior |

---

## Status

The entity and DTO models are created.

The project builds successfully.

The data model foundation supports:

- SQLite
- EF Core
- DbContext
- Migration setup
- Repository Pattern
- Watchlist CRUD endpoints
- Soft delete behavior
- Reactivation behavior
- Future price refresh features
- Future alert features
- Future action item features