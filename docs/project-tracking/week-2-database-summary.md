# Week 2 Database and CRUD Summary

This document summarizes the database, repository, and Watchlist Items CRUD implementation of the MarketInsight Operations Tracker API.

The purpose of this document is to make the database and CRUD flow understandable, testable, and demo-ready through Swagger and project documentation.

---

## Summary

The project now has a working database-backed Watchlist Items CRUD flow.

The implemented flow uses:

- SQLite database
- EF Core
- AppDbContext
- Entity models
- DTO models
- Repository Pattern
- LINQ queries
- Service layer business rules
- Controller-based API endpoints
- Swagger testing
- XML Summary documentation

The Watchlist Items API follows this route standard:

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

## Implemented Technical Scope

The implementation includes:

- Initial entity models
- Initial DTO models
- SQLite database connection
- EF Core AppDbContext setup
- EF Core migrations
- Entity relationship model
- Entity constraint standards
- Repository Pattern implementation
- LINQ-based data access
- Watchlist Items Service layer
- Watchlist Items Controller
- Watchlist Items CRUD endpoints
- Soft delete behavior
- Reactivation behavior for inactive watchlist items
- Swagger test flow
- API contract documentation

---

## Current CRUD Endpoints

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/watchlist-items` | Gets all active watchlist items |
| `GET` | `/api/watchlist-items/{symbol}` | Gets a watchlist item by symbol |
| `POST` | `/api/watchlist-items` | Creates a new watchlist item or reactivates an inactive existing item |
| `DELETE` | `/api/watchlist-items/{symbol}` | Deletes a watchlist item by symbol using soft delete behavior |

---

## Architecture Flow

The current Watchlist Items flow follows the layered architecture standard.

```text
HTTP Request
    ↓
WatchlistItemsController
    ↓
WatchlistItemService
    ↓
IWatchlistItemRepository
    ↓
WatchlistItemRepository
    ↓
AppDbContext
    ↓
SQLite Database
```

Response flow:

```text
SQLite Database
    ↓
AppDbContext
    ↓
Repository
    ↓
Service
    ↓
Controller
    ↓
HTTP Response
```

---

## Layer Responsibilities

| Layer | Responsibility |
|---|---|
| Controller | Handles HTTP routes, request entry, and response status codes |
| Service | Handles business rules, symbol normalization, active duplicate checks, inactive record reactivation, soft delete behavior, and DTO mapping |
| Repository | Handles data access through EF Core and LINQ |
| Entity | Represents database persistence models |
| DTO | Represents API request and response contracts |

Important rules:

- Controller does not contain business logic.
- Controller does not directly use `AppDbContext`.
- Controller does not return Entity models directly.
- Service owns symbol normalization, active duplicate behavior, inactive record reactivation, and soft delete behavior.
- Repository only handles data access.
- API responses use DTOs.

---

## Database Setup Summary

The project uses SQLite as the local development database.

Connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=marketinsight.db"
  }
}
```

The database context is:

```text
AppDbContext
```

The main DbSets are:

```text
WatchlistItems
PriceSnapshots
PriceAlerts
ActionItems
```

SQLite is used because it is lightweight, local, simple to run, and suitable for the current learning-focused backend project.

---

## EF Core Summary

The project uses EF Core for database access.

EF Core responsibilities in this project:

- Maps Entity classes to database tables
- Uses `AppDbContext` as the database session
- Supports LINQ queries
- Tracks entity changes
- Applies migrations
- Saves changes to SQLite

The project uses EF Core Code First approach.

This means Entity classes and DbContext configuration define the database schema.

---

## Migration Summary

EF Core migrations are used to version database schema changes.

Current migration work includes:

- Initial database schema creation
- Entity table creation
- Relationship configuration
- Constraint configuration
- Unique index configuration for `NormalizedSymbol`

Migration files should be committed to Git.

Local SQLite database files should not be committed.

---

## SQLite File Behavior

The SQLite runtime database file is local development output.

The project should not commit files such as:

```text
*.db
*.db-shm
*.db-wal
```

Reason:

- They are local runtime artifacts.
- They may contain local development data.
- They can differ between machines.
- The schema is already represented by migration files.

The database can be recreated through migrations.

---

## Entity Summary

The current core entities are:

| Entity | Purpose |
|---|---|
| `WatchlistItem` | Main tracked financial symbol record |
| `PriceSnapshot` | Stores retrieved price snapshots for a watchlist item |
| `PriceAlert` | Stores alert rules for a watchlist item |
| `ActionItem` | Stores operational follow-up actions |

The current CRUD implementation focuses on:

```text
WatchlistItem
```

---

## WatchlistItem Behavior

`WatchlistItem` is the main resource of the current CRUD flow.

Important fields:

| Field | Purpose |
|---|---|
| `Id` | Primary key |
| `Symbol` | Display symbol stored by the system |
| `NormalizedSymbol` | Symbol value used for lookup and duplicate control |
| `DisplayName` | Optional display name |
| `Market` | Optional market or exchange value |
| `IsActive` | Indicates whether the item is active |
| `CreatedAtUtc` | UTC creation time |
| `UpdatedAtUtc` | UTC update time for delete or reactivation operations |

The system uses soft delete instead of hard delete.

This means a deleted `WatchlistItem` remains in the database with:

```text
IsActive = false
```

If the same symbol is submitted again through `POST /api/watchlist-items`, the inactive existing record is reactivated instead of creating a duplicate database row.

---

## Symbol Normalization

Symbol normalization is handled in the Service layer.

Rule:

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

This makes symbol lookup and duplicate checks consistent.

---

## Duplicate Symbol Rule

The system does not allow duplicate active normalized symbols.

Example:

```text
aapl
AAPL
 AAPL
```

All become:

```text
AAPL
```

Therefore, they are treated as the same symbol.

If the same normalized symbol already exists as an active record, the API returns:

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
No existing symbol       → 201 Created
Existing active symbol   → 409 Conflict
Existing inactive symbol → 200 OK and reactivated
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

## Reactivation Behavior

Reactivation handles the case where a symbol already exists in the database but is inactive because of soft delete.

If the client sends:

```http
POST /api/watchlist-items
```

with a symbol that already exists as an inactive record, the API reactivates the existing row.

Reactivation sets:

```text
IsActive = true
UpdatedAtUtc = current UTC client sends:

```http
POST /api/watchlist-items
```

with a symbol that already exists as an inactive record, the API reactivates the existing row.

Reactivation sets time
```

Reactivation may update:

```text
DisplayName
Market
```

from the request body.

Reactivation does not change the original:

```text
CreatedAtUtc
```

Reactivation does not create a new database row.

Expected result:

```text
200 OK
```

---

## Unique Index Behavior

The database protects symbol uniqueness with a unique index on:

```text
NormalizedSymbol
```

This ensures that duplicate symbols are not inserted into the database.

The unique index remains valid because the system does not create a new row for a soft-deleted symbol.

Instead, the Service layer follows this behavior:

| Existing Record State | POST Behavior | Status Code |
|---|---|---|
| No existing record | Create a new `WatchlistItem` | `201 Created` |
| Existing record is active | Return duplicate conflict | `409 Conflict` |
| Existing record is inactive | Reactivate existing `WatchlistItem` | `200 OK` |

This gives both:

- Better API behavior
- Stronger database consistency
- No duplicate database rows for the same normalized symbol
- Cleaner soft delete lifecycle

---

## Decimal and UTC Standards

Financial values use:

```text
decimal
```

Date/time fields use UTC naming:

```text
CreatedAtUtc
UpdatedAtUtc
RetrievedAtUtc
LastTriggeredAtUtc
CompletedAtUtc
```

The project avoids:

```text
DateTime.Now
double
float
```

for finance and timestamp-sensitive behavior.

---

## Repository Pattern Summary

The WatchlistItem repository separates database access from business and HTTP logic.

Repository interface:

```text
IWatchlistItemRepository
```

Repository implementation:

```text
WatchlistItemRepository
```

Implemented repository methods:

| Method | Purpose |
|---|---|
| `GetAllActiveAsync` | Gets all active watchlist items |
| `GetByNormalizedSymbolAsync` | Gets one item by normalized symbol, including inactive records |
| `ExistsByNormalizedSymbolAsync` | Checks whether a normalized symbol exists |
| `AddAsync` | Adds a new watchlist item to the EF Core change tracker |
| `SaveChangesAsync` | Saves pending changes to the database |

Reactivation uses `GetByNormalizedSymbolAsync` so that the Service can inspect whether the existing record is active or inactive.

---

## LINQ Usage Summary

The repository uses LINQ for database queries.

Used LINQ / EF Core methods include:

| Method | Purpose |
|---|---|
| `Where` | Filters records |
| `OrderBy` | Sorts records |
| `ToListAsync` | Returns query result as a list |
| `FirstOrDefaultAsync` | Returns the first matching record or null |
| `AnyAsync` | Checks whether a matching record exists |
| `AsNoTracking` | Reads records without tracking changes |

Example:

```csharp
return await _context.WatchlistItems
    .AsNoTracking()
    .Where(item => item.IsActive)
    .OrderBy(item => item.Symbol)
    .ToListAsync();
```

---

## Service Layer Summary

The WatchlistItem service contains business and use case logic.

Service interface:

```text
IWatchlistItemService
```

Service implementation:

```text
WatchlistItemService
```

Service responsibilities:

- Normalize symbols
- Check whether a matching normalized symbol exists
- Return `409 Conflict` behavior for active duplicates
- Reactivate inactive existing watchlist items
- Create new WatchlistItem entities
- Map entities to DTO responses
- Handle soft delete behavior
- Coordinate repository calls

The Service owns the following business rules:

```text
New symbol → create new record
Active existing symbol → duplicate conflict
Inactive existing symbol → reactivate existing record
Delete active symbol → set IsActive to false
```

---

## Controller Summary

The WatchlistItemsController exposes HTTP endpoints.

Controller:

```text
WatchlistItemsController
```

Route prefix:

```text
/api/watchlist-items
```

Controller responsibilities:

- Receive HTTP requests
- Call Service methods
- Return HTTP status codes
- Return DTO responses
- Expose Swagger-documented endpoints

The Controller does not:

- Use `AppDbContext` directly
- Contain database queries
- Contain symbol normalization logic
- Contain duplicate symbol business logic
- Contain reactivation business logic
- Return Entity models directly

The Controller maps Service results to HTTP responses:

| Service Result | HTTP Response |
|---|---|
| Created | `201 Created` |
| Reactivated | `200 OK` |
| Duplicate | `409 Conflict` |

---

## CRUD Status Code Summary

| Scenario | Status Code |
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

## Swagger Demo Flow

The Watchlist Items API can be tested through Swagger UI.

For a clean manual test, use a symbol that is easy to identify, such as:

```text
MSFT
```

---

### 1. List Active Items

Request:

```http
GET /api/watchlist-items
```

Expected result:

```text
200 OK
```

Response:

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

Expected result:

```text
201 Created
```

Expected response includes:

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

Expected result:

```text
200 OK
```

Expected behavior:

```text
MSFT appears in the active watchlist item list.
```

---

### 4. Get MSFT Detail

Request:

```http
GET /api/watchlist-items/msft
```

Expected result:

```text
200 OK
```

Expected response includes:

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

Expected result:

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

Expected result:

```text
204 No Content
```

Expected behavior:

```text
The item becomes inactive.
```

---

### 7. Confirm MSFT Is No Longer Active

Request:

```http
GET /api/watchlist-items/msft
```

Expected result:

```text
404 Not Found
```

Reason:

```text
The record exists in the database but is no longer active.
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

Expected result:

```text
200 OK
```

Expected response includes:

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

Expected result:

```text
200 OK
```

Expected behavior:

```text
MSFT appears again in the active watchlist item list.
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

Expected result:

```text
409 Conflict
```

Reason:

```text
MSFT is active again, so it is treated as an active duplicate.
```

---

## Swagger Documentation

The Watchlist Items endpoints include XML Summary documentation.

Swagger shows:

- Endpoint route
- HTTP method
- Endpoint summary
- Request body schema
- Route parameter description
- Possible response status codes
- DTO schemas

This makes the API easier to test and explain.

---

## Demo Notes

During a demo, the flow can be explained as:

```text
First, the active watchlist is listed.
Then a symbol is created with lowercase input.
The API stores it as uppercase through Service-layer normalization.
The list endpoint confirms the item exists.
The detail endpoint confirms symbol lookup works.
The duplicate create request returns 409 Conflict while the item is active.
The delete endpoint performs soft delete.
The detail endpoint returns 404 because inactive records are not exposed as active resources.
When the same symbol is posted again, the existing inactive database record is reactivated.
The list endpoint confirms the symbol appears again.
A duplicate create request after reactivation returns 409 Conflict because the item is active again.
```

---

## GitHub Project Board Summary

The related implementation and documentation work should be reflected on the GitHub Project Board.

Expected board state:

| Work Item | Expected Status |
|---|---|
| Repository Pattern implementation | Done |
| Watchlist Items CRUD implementation | Done |
| Soft delete reactivation behavior | Done |
| Database and CRUD documentation | Done after this document and related documentation updates |

---

## Documentation Produced

Related documentation:

| Document | Purpose |
|---|---|
| `docs/architecture/repository-pattern-and-linq.md` | Explains Repository Pattern, LINQ, async EF Core queries, and data access separation |
| `docs/api-contracts/watchlist-items-api-contract.md` | Defines Watchlist Items API endpoints, status codes, request/response models, soft delete behavior, reactivation behavior, and Swagger test flow |
| `docs/project-tracking/week-2-database-summary.md` | Summarizes database, repository, CRUD, soft delete, reactivation, and demo behavior |
| `docs/00-index.md` | Main documentation index that references project documentation |

---

## Related Technical Documents

| Document | Purpose |
|---|---|
| `docs/architecture/project-naming-standard.md` | Defines naming standards |
| `docs/architecture/layer-responsibility-standard.md` | Defines layer responsibilities |
| `docs/architecture/api-route-naming-standard.md` | Defines API route standards |
| `docs/database-design/entity-design.md` | Explains Entity and DTO design |
| `docs/database-design/ef-core-sqlite-setup.md` | Documents EF Core and SQLite setup |
| `docs/database-design/entity-relationship-model.md` | Explains entity relationships |
| `docs/database-design/entity-constraint-standards.md` | Defines uniqueness, normalization, decimal, UTC, soft delete, and reactivation standards |

---

## Final Verification Checklist

| Check | Status |
|---|---|
| SQLite database setup exists | Done |
| EF Core AppDbContext exists | Done |
| Entity models exist | Done |
| DTO models exist | Done |
| Repository Pattern is implemented | Done |
| LINQ queries are used | Done |
| Service layer is implemented | Done |
| Controller endpoints are implemented | Done |
| `/api/watchlist-items` route standard is used | Done |
| Entity models are not returned directly from Controller | Done |
| Symbol normalization works | Done |
| New symbol returns `201 Created` | Done |
| Active duplicate symbol returns `409 Conflict` | Done |
| Delete endpoint returns `204 No Content` | Done |
| Deleted symbol returns `404 Not Found` from detail endpoint | Done |
| Inactive watchlist item can be reactivated by POST | Done |
| Reactivation returns `200 OK` | Done |
| Reactivation does not create a duplicate database row | Done |
| Reactivated item appears again in active list | Done |
| Duplicate request after reactivation returns `409 Conflict` | Done |
| Missing symbol returns `404 Not Found` | Done |
| Swagger test flow is documented | Done |
| Project builds successfully | Done |

---

## Summary

The database and Watchlist Items CRUD flow is now demo-ready.

The project has a working backend flow from HTTP endpoint to SQLite database:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
AppDbContext
    ↓
SQLite
```

The implementation follows the project standards for:

- Layered architecture
- Repository Pattern
- DTO and Entity separation
- API route naming
- Symbol normalization
- Active duplicate symbol handling
- Soft delete behavior
- Inactive record reactivation
- Unique `NormalizedSymbol` database protection
- UTC timestamps
- SQLite database usage
- Swagger-based testing

The final WatchlistItem create behavior is:

```text
New symbol → 201 Created
Active existing symbol → 409 Conflict
Inactive existing symbol → 200 OK and reactivated
```

This completes the database, repository, Watchlist Items CRUD, soft delete, and reactivation documentation scope.