# Watchlist Items API Contract

This document defines the API contract for Watchlist Items CRUD operations in the MarketInsight Operations Tracker API.

The purpose of this document is to describe the available endpoints, request models, response models, status codes, validation behavior, soft delete behavior, reactivation behavior, and Swagger test flow for watchlist item management.

---

## Purpose

Watchlist Items API endpoints allow the client to manage financial symbols tracked by the system.

The API supports:

- Listing active watchlist items
- Getting a watchlist item by symbol
- Creating a new watchlist item
- Reactivating a previously deleted watchlist item
- Deleting a watchlist item by symbol

The current API contract follows the project route naming standard:

```text
/api/watchlist-items
```

The project does not use:

```text
/api/watchlist
```

Reason:

`WatchlistItem` is the actual resource managed by the API.

---

## Resource Name

| Concept | Value |
|---|---|
| Domain resource | `WatchlistItem` |
| Controller | `WatchlistItemsController` |
| Route prefix | `/api/watchlist-items` |
| Request DTO | `CreateWatchlistItemRequest` |
| Response DTO | `WatchlistItemResponse` |

---

## Layer Responsibility

The Watchlist Items API follows Controller, Service, Repository, Entity, and DTO separation.

| Layer | Responsibility |
|---|---|
| Controller | Handles HTTP requests, response status codes, and routing |
| Service | Handles business rules, symbol normalization, duplicate checks, reactivation behavior, soft delete behavior, and DTO mapping |
| Repository | Handles data access through EF Core and LINQ |
| Entity | Represents database persistence model |
| DTO | Represents API request and response contract |

The Controller must not contain business logic.

The Controller must not directly use `AppDbContext`.

The Controller must not return Entity models directly.

The Service owns symbol normalization, active duplicate detection, inactive record reactivation, and soft delete behavior.

The Repository only handles database access.

---

## Endpoint Summary

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/watchlist-items` | Gets all active watchlist items |
| `GET` | `/api/watchlist-items/{symbol}` | Gets a watchlist item by symbol |
| `POST` | `/api/watchlist-items` | Creates a new watchlist item or reactivates an inactive existing item |
| `DELETE` | `/api/watchlist-items/{symbol}` | Deletes a watchlist item by symbol using soft delete behavior |

---

## Request and Response Format

The API uses JSON request and response bodies.

Request content type:

```text
application/json
```

Response content type:

```text
application/json
```

Some endpoints may return no response body, such as successful delete operations.

---

## DTO Standard

The API should use DTOs for request and response contracts.

Current DTOs:

```text
CreateWatchlistItemRequest
WatchlistItemResponse
```

Entity models should remain inside the persistence and service flow.

The API response should not expose Entity models directly.

---

## CreateWatchlistItemRequest

Used by:

```text
POST /api/watchlist-items
```

Example request:

```json
{
  "symbol": "aapl",
  "displayName": "Apple Inc.",
  "market": "NASDAQ"
}
```

### Fields

| Field | Type | Required | Description |
|---|---|---:|---|
| `symbol` | string | Yes | Financial symbol requested by the user |
| `displayName` | string | No | Optional display name for the symbol |
| `market` | string | No | Optional market or exchange information |

---

## WatchlistItemResponse

Returned by:

```text
GET /api/watchlist-items
GET /api/watchlist-items/{symbol}
POST /api/watchlist-items
```

Example response:

```json
{
  "id": 1,
  "symbol": "AAPL",
  "normalizedSymbol": "AAPL",
  "displayName": "Apple Inc.",
  "market": "NASDAQ",
  "isActive": true,
  "createdAtUtc": "2026-05-28T17:16:48.8081171Z",
  "updatedAtUtc": null
}
```

### Fields

| Field | Type | Description |
|---|---|---|
| `id` | integer | Unique database identifier |
| `symbol` | string | Display symbol stored by the system |
| `normalizedSymbol` | string | Normalized symbol used for lookup and duplicate checks |
| `displayName` | string or null | Optional display name |
| `market` | string or null | Optional market or exchange |
| `isActive` | boolean | Indicates whether the item is active |
| `createdAtUtc` | datetime | UTC creation time |
| `updatedAtUtc` | datetime or null | UTC update time if the item was updated, deleted, or reactivated |

---

## Symbol Normalization

Symbols are normalized in the Service layer.

Normalization rule:

```text
Trim whitespace.
Convert to uppercase.
```

Examples:

| Input | Normalized Value |
|---|---|
| `aapl` | `AAPL` |
| `Aapl` | `AAPL` |
| ` AAPL ` | `AAPL` |
| `msft` | `MSFT` |

The Repository does not decide normalization behavior.

The Controller does not decide normalization behavior.

The Service owns this business rule.

---

## Duplicate Symbol Rule

The system does not allow duplicate active normalized symbols.

If the client tries to add the same symbol while an active record already exists, the API returns:

```text
409 Conflict
```

Example:

First request:

```json
{
  "symbol": "aapl",
  "displayName": "Apple Inc.",
  "market": "NASDAQ"
}
```

Stored value:

```text
AAPL
```

Second request while AAPL is still active:

```json
{
  "symbol": "AAPL",
  "displayName": "Apple Inc.",
  "market": "NASDAQ"
}
```

Expected result:

```text
409 Conflict
```

Example response:

```json
{
  "message": "Watchlist item with symbol 'AAPL' already exists."
}
```

Important distinction:

```text
Active existing symbol   → 409 Conflict
Inactive existing symbol → 200 OK and reactivated
No existing symbol       → 201 Created
```

---

## Soft Delete Behavior

Delete operation uses soft delete behavior.

When a watchlist item is deleted:

```text
IsActive = false
UpdatedAtUtc = current UTC time
```

The record remains in the database.

The item no longer appears in active list results.

The item no longer returns from the detail endpoint as an active item.

This means:

```text
GET /api/watchlist-items
```

only returns active records.

A soft-deleted record may still exist in the database even if it is not visible in the active list.

---

## Re-adding Deleted Symbols

The project uses a unique constraint on `NormalizedSymbol`.

This means the same normalized symbol should not be inserted again as a new database row.

Updated behavior:

```text
Deleted symbol + POST same symbol again = 200 OK and reactivated item
```

If an existing `WatchlistItem` has the same `NormalizedSymbol` but `IsActive = false`, the API reactivates the existing record instead of creating a duplicate row or returning `409 Conflict`.

Reactivation behavior:

```text
IsActive = true
UpdatedAtUtc = current UTC time
DisplayName may be updated from the request
Market may be updated from the request
```

The original `CreatedAtUtc` remains the original creation time.

The database row is reused.

No duplicate `WatchlistItem` row is created.

---

## POST Behavior Decision Table

| Existing Record State | POST Behavior | Status Code |
|---|---|---|
| No existing record with the same `NormalizedSymbol` | Create a new `WatchlistItem` | `201 Created` |
| Existing record is active | Return duplicate conflict | `409 Conflict` |
| Existing record is inactive | Reactivate the existing `WatchlistItem` | `200 OK` |

---

## GET /api/watchlist-items

Gets all active watchlist items.

### Request

```http
GET /api/watchlist-items
```

### Success Response

Status code:

```text
200 OK
```

Example response on a fresh database or when all records are inactive:

```json
[]
```

Example response with active items:

```json
[
  {
    "id": 1,
    "symbol": "AAPL",
    "normalizedSymbol": "AAPL",
    "displayName": "Apple Inc.",
    "market": "NASDAQ",
    "isActive": true,
    "createdAtUtc": "2026-05-28T17:16:48.8081171Z",
    "updatedAtUtc": null
  }
]
```

### Notes

This endpoint only returns active watchlist items.

Inactive records should not be returned.

A symbol not appearing in this list may still exist in the database as an inactive soft-deleted record.

---

## GET /api/watchlist-items/{symbol}

Gets a single active watchlist item by symbol.

### Request

```http
GET /api/watchlist-items/aapl
```

### Route Parameters

| Parameter | Type | Required | Description |
|---|---|---:|---|
| `symbol` | string | Yes | Requested financial symbol |

### Success Response

Status code:

```text
200 OK
```

Example response:

```json
{
  "id": 1,
  "symbol": "AAPL",
  "normalizedSymbol": "AAPL",
  "displayName": "Apple Inc.",
  "market": "NASDAQ",
  "isActive": true,
  "createdAtUtc": "2026-05-28T17:16:48.8081171Z",
  "updatedAtUtc": null
}
```

### Not Found Response

Status code:

```text
404 Not Found
```

Example response:

```json
{
  "message": "Watchlist item with symbol 'AAPL' was not found."
}
```

### Notes

The route accepts lowercase or mixed-case symbols.

The Service normalizes the symbol before lookup.

Example:

```text
aapl → AAPL
```

This endpoint only returns active watchlist items.

If a matching record exists but `IsActive = false`, the endpoint returns:

```text
404 Not Found
```

---

## POST /api/watchlist-items

Creates a new watchlist item or reactivates an inactive existing watchlist item.

### Request

```http
POST /api/watchlist-items
```

### Request Body

```json
{
  "symbol": "aapl",
  "displayName": "Apple Inc.",
  "market": "NASDAQ"
}
```

---

### New Item Success Response

Used when no existing record has the same `NormalizedSymbol`.

Status code:

```text
201 Created
```

Example response:

```json
{
  "id": 1,
  "symbol": "AAPL",
  "normalizedSymbol": "AAPL",
  "displayName": "Apple Inc.",
  "market": "NASDAQ",
  "isActive": true,
  "createdAtUtc": "2026-05-28T17:16:48.8081171Z",
  "updatedAtUtc": null
}
```

### Location Header

A successful new create response should include a location header pointing to the created resource.

Example:

```text
/api/watchlist-items/AAPL
```

---

### Reactivation Success Response

Used when a matching record exists but `IsActive = false`.

Status code:

```text
200 OK
```

Example response:

```json
{
  "id": 1,
  "symbol": "AAPL",
  "normalizedSymbol": "AAPL",
  "displayName": "Apple Inc.",
  "market": "NASDAQ",
  "isActive": true,
  "createdAtUtc": "2026-05-28T17:16:48.8081171Z",
  "updatedAtUtc": "2026-05-28T18:00:00.0000000Z"
}
```

### Reactivation Notes

Reactivation does not create a new database row.

The existing inactive record is reused.

The API sets:

```text
IsActive = true
UpdatedAtUtc = current UTC time
```

The API may update:

```text
DisplayName
Market
```

from the request body.

---

### Bad Request Response

Status code:

```text
400 Bad Request
```

Possible reason:

```text
Required request fields are missing or invalid.
```

Example invalid request:

```json
{
  "symbol": "",
  "displayName": "Apple Inc.",
  "market": "NASDAQ"
}
```

---

### Conflict Response

Status code:

```text
409 Conflict
```

Possible reason:

```text
A watchlist item with the same normalized symbol already exists and is active.
```

Example response:

```json
{
  "message": "Watchlist item with symbol 'AAPL' already exists."
}
```

---

## DELETE /api/watchlist-items/{symbol}

Deletes a watchlist item by symbol.

The current implementation uses soft delete behavior.

### Request

```http
DELETE /api/watchlist-items/aapl
```

### Route Parameters

| Parameter | Type | Required | Description |
|---|---|---:|---|
| `symbol` | string | Yes | Requested financial symbol |

### Success Response

Status code:

```text
204 No Content
```

Response body:

```text
No response body.
```

### Not Found Response

Status code:

```text
404 Not Found
```

Example response:

```json
{
  "message": "Watchlist item with symbol 'AAPL' was not found."
}
```

### Notes

After successful delete:

```text
IsActive = false
UpdatedAtUtc = current UTC time
```

The item should not appear in:

```text
GET /api/watchlist-items
```

The item should not return as active from:

```text
GET /api/watchlist-items/{symbol}
```

If the same symbol is submitted again through `POST /api/watchlist-items`, the inactive record should be reactivated.

---

## Status Code Summary

| Scenario | Expected Status Code |
|---|---|
| Get active watchlist items | `200 OK` |
| Get existing active symbol | `200 OK` |
| Get missing symbol | `404 Not Found` |
| Get inactive symbol | `404 Not Found` |
| Create new valid watchlist item | `201 Created` |
| Reactivate inactive existing watchlist item | `200 OK` |
| Create invalid request | `400 Bad Request` |
| Create active duplicate symbol | `409 Conflict` |
| Delete existing active symbol | `204 No Content` |
| Delete missing symbol | `404 Not Found` |
| Delete inactive symbol | `404 Not Found` |

---

## Swagger Test Flow

Use Swagger UI to manually test the Watchlist Items API.

For a clean manual test, use a symbol that is easy to identify, such as:

```text
MSFT
```

The same flow also applies to any other symbol.

---

### 1. List Active Items

Request:

```http
GET /api/watchlist-items
```

Expected status:

```text
200 OK
```

Expected response:

```json
[]
```

or a list of active items if data already exists.

---

### 2. Create MSFT

Request:

```http
POST /api/watchlist-items
```

Body:

```json
{
  "symbol": "msft",
  "displayName": "Microsoft Corporation",
  "market": "NASDAQ"
}
```

Expected status:

```text
201 Created
```

Expected response should include:

```json
"symbol": "MSFT"
```

This confirms symbol normalization.

---

### 3. List Items Again

Request:

```http
GET /api/watchlist-items
```

Expected status:

```text
200 OK
```

Expected result:

```text
MSFT appears in the active item list.
```

---

### 4. Get MSFT Detail

Request:

```http
GET /api/watchlist-items/msft
```

Expected status:

```text
200 OK
```

Expected response should include:

```json
"symbol": "MSFT"
```

This confirms normalized lookup.

---

### 5. Try Duplicate MSFT While Active

Request:

```http
POST /api/watchlist-items
```

Body:

```json
{
  "symbol": "MSFT",
  "displayName": "Microsoft Corporation",
  "market": "NASDAQ"
}
```

Expected status:

```text
409 Conflict
```

Expected response:

```json
{
  "message": "Watchlist item with symbol 'MSFT' already exists."
}
```

Reason:

```text
MSFT already exists and is active.
```

---

### 6. Delete MSFT

Request:

```http
DELETE /api/watchlist-items/msft
```

Expected status:

```text
204 No Content
```

Expected behavior:

```text
MSFT becomes inactive.
```

---

### 7. Get MSFT After Delete

Request:

```http
GET /api/watchlist-items/msft
```

Expected status:

```text
404 Not Found
```

Reason:

```text
The item is inactive after soft delete.
```

---

### 8. Re-add MSFT After Delete

Request:

```http
POST /api/watchlist-items
```

Body:

```json
{
  "symbol": "msft",
  "displayName": "Microsoft Corporation",
  "market": "NASDAQ"
}
```

Expected status:

```text
200 OK
```

Expected response should include:

```json
"isActive": true
```

Reason:

```text
The existing inactive MSFT record is reactivated instead of creating a duplicate row.
```

---

### 9. List Items After Reactivation

Request:

```http
GET /api/watchlist-items
```

Expected status:

```text
200 OK
```

Expected result:

```text
MSFT appears again in the active item list.
```

---

### 10. Try Duplicate MSFT After Reactivation

Request:

```http
POST /api/watchlist-items
```

Body:

```json
{
  "symbol": "MSFT",
  "displayName": "Microsoft Corporation",
  "market": "NASDAQ"
}
```

Expected status:

```text
409 Conflict
```

Reason:

```text
MSFT is active again, so it is treated as an active duplicate.
```

---

## Validation Behavior

Validation happens in multiple places.

| Validation Area | Layer | Example |
|---|---|---|
| Required field validation | DTO / Model binding | `symbol` is required |
| Symbol normalization | Service | `msft` becomes `MSFT` |
| Active duplicate symbol rule | Service | Existing active normalized symbol returns conflict |
| Inactive symbol reactivation rule | Service | Existing inactive normalized symbol is reactivated |
| Unique symbol protection | Database | Unique index on `NormalizedSymbol` |

---

## Out of Scope

The following features are not part of the current Watchlist Items CRUD contract:

- External finance API integration
- Price refresh
- Redis cache
- RabbitMQ messaging
- Background Worker
- Alert evaluation
- Price snapshot creation
- Multiple watchlists
- Hard delete

These features will be handled separately.

---

## Related Documents

| Document | Purpose |
|---|---|
| `docs/architecture/api-route-naming-standard.md` | Defines route naming standards |
| `docs/architecture/layer-responsibility-standard.md` | Defines layer responsibilities |
| `docs/architecture/repository-pattern-and-linq.md` | Explains repository and LINQ usage |
| `docs/database-design/entity-design.md` | Explains entity and DTO model design |
| `docs/database-design/entity-constraint-standards.md` | Defines symbol uniqueness, normalization, decimal, UTC standards, soft delete, and reactivation behavior |

---

## Summary

The Watchlist Items API provides the basic CRUD flow for tracked financial symbols.

Current resource route:

```text
/api/watchlist-items
```

The implementation follows these rules:

- Controller handles HTTP routing and status codes.
- Service handles business rules, reactivation behavior, soft delete behavior, and DTO mapping.
- Repository handles data access.
- Entity models are not returned directly.
- Symbols are normalized before lookup and persistence.
- A new normalized symbol returns `201 Created`.
- An active duplicate normalized symbol returns `409 Conflict`.
- An inactive existing normalized symbol is reactivated and returns `200 OK`.
- Delete operation uses soft delete behavior.
- Soft-deleted records remain in the database but are not returned as active resources.
- Reactivation reuses the existing database row instead of creating a duplicate row.