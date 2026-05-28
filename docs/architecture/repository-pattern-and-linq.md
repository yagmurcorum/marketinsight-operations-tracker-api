# Repository Pattern and LINQ

This document explains how Repository Pattern and LINQ are used in the MarketInsight Operations Tracker API.

The purpose of this document is to define how database access is separated from HTTP and business logic.

---

## Purpose

The project uses Repository Pattern to keep database access responsibilities in a dedicated layer.

This supports the learning-focused layered monolith architecture used in the project.

Repository Pattern helps keep the codebase easier to understand, test, maintain, and extend.

---

## Why Repository Pattern Is Used

Repository Pattern is used to separate database access from API and business logic.

Without a repository, Controllers may directly use `AppDbContext`.

That would make Controllers responsible for too many things.

A Controller should not know how the database is queried.

A Controller should only handle HTTP request and response flow.

---

## Layer Responsibility Reminder

The project separates responsibilities as follows:

| Layer | Main Responsibility |
|---|---|
| Controller | Handles HTTP requests and responses |
| Service | Handles use cases and business rules |
| Repository | Handles database access |
| Entity | Represents database persistence model |
| DTO | Represents API request and response contract |

Detailed layer responsibilities are documented in:

```text
docs/architecture/layer-responsibility-standard.md
```

---

## Repository Responsibility

Repository is responsible for data access.

In this project, the repository:

- Uses `AppDbContext`
- Uses EF Core queries
- Uses LINQ
- Uses async database methods
- Reads data from the database
- Adds data to the database context
- Saves database changes

---

## What Repository Does Not Do

Repository should not contain business logic.

Repository should not:

- Decide HTTP status codes
- Return `Ok`, `NotFound`, `Conflict`, or `BadRequest`
- Normalize symbols as a business rule
- Decide duplicate symbol behavior
- Map Entity models to DTOs for API responses
- Handle Swagger behavior
- Handle request validation messages

Those responsibilities belong to Controller or Service layers.

---

## Why Controller Should Not Use AppDbContext Directly

Controllers should not directly access the database.

Bad approach:

```csharp
public class WatchlistItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WatchlistItemsController(AppDbContext context)
    {
        _context = context;
    }
}
```

This approach mixes HTTP responsibility with data access responsibility.

Better approach:

```csharp
public class WatchlistItemsController : ControllerBase
{
    private readonly IWatchlistItemService _service;

    public WatchlistItemsController(IWatchlistItemService service)
    {
        _service = service;
    }
}
```

Controller should call the Service.

Service should coordinate business rules.

Repository should access the database.

---

## Expected Request Flow

The expected request flow is:

```text
HTTP Request
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
AppDbContext
    ↓
SQLite Database
```

The response flow returns in the opposite direction:

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

## Repository Interface

The project uses an interface for WatchlistItem data access:

```csharp
IWatchlistItemRepository
```

The interface defines what operations are available.

It does not define how the database works internally.

This makes the data access contract clear.

---

## Repository Implementation

The concrete repository implementation is:

```csharp
WatchlistItemRepository
```

This class uses:

```csharp
AppDbContext
```

and EF Core LINQ methods to read and write database records.

---

## Implemented Repository Methods

The repository implementation includes the following methods:

| Method | Purpose |
|---|---|
| `GetAllActiveAsync` | Gets all active watchlist items |
| `GetByNormalizedSymbolAsync` | Gets one item by normalized symbol |
| `ExistsByNormalizedSymbolAsync` | Checks whether a symbol already exists |
| `AddAsync` | Adds a new watchlist item to the database context |
| `SaveChangesAsync` | Saves pending database changes |

---

## GetAllActiveAsync

This method returns active watchlist items.

```csharp
Task<List<WatchlistItem>> GetAllActiveAsync();
```

Expected behavior:

- Reads from `WatchlistItems`
- Filters by `IsActive`
- Orders by `Symbol`
- Uses `ToListAsync`
- Uses `AsNoTracking` because this is a read-only query

Repository responsibility:

```text
Read active records from the database.
```

Not repository responsibility:

```text
Decide HTTP 200 OK response.
Map Entity to DTO.
```

---

## GetByNormalizedSymbolAsync

This method returns one watchlist item by normalized symbol.

```csharp
Task<WatchlistItem?> GetByNormalizedSymbolAsync(string normalizedSymbol);
```

Expected behavior:

- Searches by `NormalizedSymbol`
- Returns the matching item if found
- Returns null if not found
- Uses `FirstOrDefaultAsync`

Important note:

This method does not filter only active records.

Reason:

The project uses a unique constraint on `NormalizedSymbol`.

Future soft delete or reactivation behavior may need to find inactive records too.

---

## ExistsByNormalizedSymbolAsync

This method checks whether a normalized symbol already exists.

```csharp
Task<bool> ExistsByNormalizedSymbolAsync(string normalizedSymbol);
```

Expected behavior:

- Searches by `NormalizedSymbol`
- Returns `true` if the symbol exists
- Returns `false` if it does not exist
- Uses `AnyAsync`

This method helps the Service layer apply duplicate symbol rules.

The Repository only checks existence.

The Service decides what to do with that result.

---

## AddAsync

This method adds a new WatchlistItem entity to the database context.

```csharp
Task AddAsync(WatchlistItem watchlistItem);
```

Important note:

`AddAsync` does not immediately save changes to the database.

It only tracks the new entity in the EF Core change tracker.

The actual database write happens when `SaveChangesAsync` is called.

---

## SaveChangesAsync

This method persists pending changes to the database.

```csharp
Task SaveChangesAsync();
```

Expected behavior:

- Calls `AppDbContext.SaveChangesAsync`
- Writes tracked changes to the SQLite database

---

## LINQ Usage

LINQ means Language Integrated Query.

In this project, LINQ is used to query Entity collections through EF Core.

Example:

```csharp
_context.WatchlistItems
    .Where(item => item.IsActive)
    .OrderBy(item => item.Symbol)
    .ToListAsync();
```

This query means:

```text
Get watchlist items.
Filter only active records.
Order them by symbol.
Return the result as a list asynchronously.
```

---

## Common LINQ Methods Used

| Method | Purpose |
|---|---|
| `Where` | Filters records |
| `OrderBy` | Sorts records |
| `FirstOrDefaultAsync` | Gets the first matching record or null |
| `AnyAsync` | Checks whether any matching record exists |
| `ToListAsync` | Converts query result to a list |

---

## Where

`Where` is used to filter records.

Example:

```csharp
Where(item => item.IsActive)
```

This means:

```text
Only return records where IsActive is true.
```

---

## OrderBy

`OrderBy` is used to sort records.

Example:

```csharp
OrderBy(item => item.Symbol)
```

This means:

```text
Sort records by Symbol in ascending order.
```

---

## FirstOrDefaultAsync

`FirstOrDefaultAsync` is used when the system expects zero or one matching result.

Example:

```csharp
FirstOrDefaultAsync(item => item.NormalizedSymbol == normalizedSymbol)
```

Possible results:

| Situation | Result |
|---|---|
| Matching record exists | Returns the entity |
| Matching record does not exist | Returns null |

This is useful for detail endpoints and delete flows.

---

## AnyAsync

`AnyAsync` is used when the system only needs to know whether a record exists.

Example:

```csharp
AnyAsync(item => item.NormalizedSymbol == normalizedSymbol)
```

Possible results:

| Situation | Result |
|---|---|
| Matching record exists | `true` |
| Matching record does not exist | `false` |

This is useful for duplicate checks.

---

## ToListAsync

`ToListAsync` executes a query and returns the result as a list.

Example:

```csharp
ToListAsync()
```

This is useful for list endpoints.

---

## AsNoTracking

`AsNoTracking` is used for read-only queries.

Example:

```csharp
_context.WatchlistItems
    .AsNoTracking()
```

Reason:

If the application only reads data and does not update the returned entities, EF Core does not need to track them.

This can make read operations lighter.

---

## Async Database Access

The repository uses async methods.

Examples:

```csharp
ToListAsync()
FirstOrDefaultAsync()
AnyAsync()
AddAsync()
SaveChangesAsync()
```

Async methods help the application avoid blocking the request thread during database operations.

This is especially important in web APIs.

---

## Dependency Injection Registration

The repository is registered in `Program.cs`.

```csharp
builder.Services.AddScoped<IWatchlistItemRepository, WatchlistItemRepository>();
```

This means:

- When a class asks for `IWatchlistItemRepository`, the application provides `WatchlistItemRepository`.
- A scoped instance is created per HTTP request.
- This lifetime works well with EF Core `DbContext`.

---

## Current Implementation Files

The repository implementation includes:

```text
src/MarketInsight.Api/Repositories/IWatchlistItemRepository.cs
src/MarketInsight.Api/Repositories/WatchlistItemRepository.cs
```

The dependency injection registration is located in:

```text
src/MarketInsight.Api/Program.cs
```

---

## Relationship With Service Layer

The Repository does not directly communicate with Controllers in the final CRUD flow.

Expected flow:

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

The Service layer will use the Repository to implement use cases such as:

- List watchlist items
- Get watchlist item detail
- Add a new symbol
- Check duplicate symbol
- Soft delete a symbol

---

## Repository and Business Rules

The Repository should not decide business behavior.

Example:

Repository can answer:

```text
Does AAPL exist?
```

Service decides:

```text
If AAPL exists, return duplicate conflict result.
```

This keeps the project architecture clean.

---

## Repository and DTOs

Repository should not return DTOs.

Repository should return Entities.

Reason:

Repository works with database models.

DTO mapping belongs to the Service layer or a dedicated mapping layer.

Current rule:

```text
Repository returns Entity models.
Controller returns DTO models.
```

---

## Repository and HTTP Status Codes

Repository should not know HTTP concepts.

Repository should not return:

```text
200 OK
201 Created
204 No Content
404 Not Found
409 Conflict
```

These are Controller-level response concerns.

---

## Implementation Outcome

After this implementation:

- Repository folder was created.
- `IWatchlistItemRepository` was created.
- `WatchlistItemRepository` was created.
- Repository methods were implemented with async EF Core queries.
- LINQ was used for filtering, ordering, lookup, and existence checks.
- Repository was registered in dependency injection.
- Project build completed successfully.

---

## Review Questions

1. Why should Controller not directly use `AppDbContext`?
2. What responsibility does Repository have?
3. What responsibility does Repository not have?
4. What is the difference between Service and Repository?
5. Why is `IWatchlistItemRepository` useful?
6. What does `FirstOrDefaultAsync` do?
7. What does `AnyAsync` do?
8. What does `ToListAsync` do?
9. Why do we use async database methods?
10. Why should Repository not return DTOs?

---

## Summary

Repository Pattern helps the project keep database access separate from API and business logic.

In this project:

- Controller handles HTTP.
- Service handles use cases and business rules.
- Repository handles data access.
- Entity represents database structure.
- DTO represents API contract.

This repository implementation prepares the project for Watchlist Items CRUD development.