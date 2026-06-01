# Entity Constraint Standards

This document defines the entity-level constraint and data consistency standards for the MarketInsight Operations Tracker API.

The purpose of this document is to clarify important database and validation decisions before implementing and maintaining Repository Pattern, LINQ queries, Watchlist CRUD endpoints, soft delete behavior, and reactivation behavior.

---

## Purpose

Entity constraint standards help the project keep data consistent, predictable, and easy to maintain.

This document defines standards for:

- Symbol uniqueness
- Symbol normalization
- Soft delete behavior
- Reactivation behavior
- Financial value types
- UTC date/time fields
- Entity validation responsibilities
- Database-level and API-level rule enforcement
- Git handling of database-related files

These standards should be followed before and during Watchlist CRUD implementation.

---

## Why Entity Constraints Matter

Without clear entity constraints, the project can develop inconsistent data behavior.

Examples of possible problems:

- The same symbol may be added multiple times.
- `aapl`, `Aapl`, and `AAPL` may be treated as different symbols.
- A soft-deleted symbol may disappear from the active list but still block future create requests.
- A deleted symbol may create confusing API behavior if reactivation is not defined.
- Financial values may lose precision if `double` or `float` is used.
- Date values may become inconsistent if local time and UTC are mixed.
- API validation may allow data that the database should reject.
- Database rules and application rules may conflict.

This document prevents these problems by defining clear standards.

---

## Constraint Summary

| Area | Standard |
|---|---|
| Symbol uniqueness | One normalized symbol should exist only once in the table |
| Symbol normalization | Trim whitespace and convert to uppercase |
| Soft delete | Use `IsActive = false` instead of hard delete |
| Reactivation | Reuse inactive existing record when the same symbol is posted again |
| Financial values | Use `decimal` |
| Date/time values | Use UTC and name fields with `Utc` suffix |
| Entity response exposure | Do not return Entity models directly from API endpoints |
| Database file tracking | Do not commit local SQLite database files |
| Migration tracking | Commit migration files |

---

## Symbol Uniqueness Standard

### Decision

The same financial symbol should not be added to the watchlist more than once as separate database rows.

The project should treat these values as the same symbol:

    aapl
    Aapl
    AAPL
    " AAPL "

After normalization, all of them become:

    AAPL

Therefore, only one `WatchlistItem` database row should exist for:

    AAPL

If that row is active, a duplicate create request should return:

    409 Conflict

If that row is inactive because of soft delete, a create request for the same symbol should reactivate the existing row instead of inserting a new row.

---

## Why Symbol Should Be Unique

The watchlist represents tracked financial symbols.

If the same symbol is added multiple times, the system may produce duplicate behavior.

Possible problems:

- Duplicate watchlist records
- Duplicate price refresh operations
- Duplicate price snapshots
- Duplicate alerts
- Confusing API responses
- Incorrect future analysis or reporting
- Harder data cleanup
- Unclear historical lifecycle for deleted and re-added symbols

---

## Standard Rule

The project should enforce symbol uniqueness using the normalized symbol value.

Main rule:

    NormalizedSymbol must be unique.

This means the system should use `NormalizedSymbol` for lookup, duplicate checks, and reactivation checks.

The database should not contain multiple rows for the same normalized symbol.

---

## Symbol Fields

| Field | Purpose |
|---|---|
| `Symbol` | Stores the display symbol value used by the API response |
| `NormalizedSymbol` | Stores the normalized value used for lookup, comparison, duplicate control, and reactivation |

Example:

| User Input | Stored Symbol | Stored NormalizedSymbol |
|---|---|---|
| `aapl` | `AAPL` | `AAPL` |
| `Aapl` | `AAPL` | `AAPL` |
| ` AAPL ` | `AAPL` | `AAPL` |
| `msft` | `MSFT` | `MSFT` |

---

## Symbol Normalization Standard

### Decision

Symbol values should be normalized before being persisted or queried.

Normalization rule:

    Trim whitespace.
    Convert to uppercase.

Example logic:

    Input: " aapl "
    Trim: "aapl"
    Uppercase: "AAPL"

Result:

    AAPL

---

## Why Normalize Symbols

Symbol normalization keeps data consistent.

Without normalization, the database may treat these as different values:

    aapl
    Aapl
    AAPL

But from a business perspective, they represent the same financial symbol.

Normalization ensures that all comparisons use one standard format.

---

## Where Symbol Normalization Should Happen

Symbol normalization is business/application behavior.

It should be handled before persistence and before lookup.

Recommended placement:

| Layer | Responsibility |
|---|---|
| Controller | Receives the raw request |
| Service | Normalizes the symbol and decides business behavior |
| Repository | Uses normalized value for query/persistence |
| Database | Stores normalized value and enforces uniqueness |

The Controller should not own normalization logic.

The Repository should not decide the business meaning of normalization.

The Service layer should coordinate this rule.

---

## Symbol Normalization Flow

    User sends symbol
          ↓
    Controller receives request DTO
          ↓
    Service trims and uppercases symbol
          ↓
    Service checks existing record using repository
          ↓
    Repository queries by NormalizedSymbol
          ↓
    Service decides create, duplicate conflict, or reactivation
          ↓
    Entity is saved or updated

---

## Example Create Flow

Input request:

    {
      "symbol": " aapl ",
      "displayName": "Apple Inc.",
      "market": "NASDAQ"
    }

Service normalization:

    " aapl " → "AAPL"

Entity values:

    Symbol = "AAPL"
    NormalizedSymbol = "AAPL"

If no existing record exists:

    Create new WatchlistItem
    Return 201 Created

If an active record exists:

    Return 409 Conflict

If an inactive record exists:

    Reactivate existing WatchlistItem
    Return 200 OK

---

## API-Level Validation Standard

API-level validation should reject clearly invalid input before database persistence.

For `CreateWatchlistItemRequest`, validation should include:

| Field | Standard |
|---|---|
| `Symbol` | Required |
| `Symbol` | Maximum length should be enforced |
| `DisplayName` | Optional |
| `DisplayName` | Maximum length should be enforced |
| `Market` | Optional |
| `Market` | Maximum length should be enforced |

Example:

    Symbol must not be empty.
    Symbol must not exceed the configured maximum length.

---

## Database-Level Constraint Standard

Some rules should also exist at the database level.

Reason:

    API validation protects normal application flow.
    Database constraints protect stored data consistency.

For symbol uniqueness, the database should enforce uniqueness on the normalized symbol value.

Current database-level rule:

    Unique index on NormalizedSymbol

This is configured through EF Core and represented by migration files.

---

## Soft Delete and Symbol Uniqueness

The project uses `IsActive` to represent active/passive watchlist records.

Deletion is implemented as soft delete:

    IsActive = false
    UpdatedAtUtc = current UTC time

Soft delete means the database row remains in the table.

The active list endpoint should not return inactive records.

The detail endpoint should not return inactive records as active resources.

However, inactive records still exist in the database and can be used for reactivation.

---

## Option 1: Unique NormalizedSymbol Across All Records

Rule:

    A symbol can exist only once in the table, even if inactive.

Pros:

- Simple.
- Easy to understand.
- Prevents historical duplicate rows.
- Keeps one lifecycle per symbol.
- Works well with SQLite.
- Keeps the unique index simple.

Cons:

- Re-adding a deleted symbol requires reactivating the old record instead of inserting a new one.
- Application logic must distinguish active duplicates from inactive records.

This is the current project decision.

---

## Option 2: Unique Active NormalizedSymbol

Rule:

    A symbol can exist only once while active.

Pros:

- Allows historical inactive records.
- Allows re-adding a symbol after deletion as a new row.

Cons:

- Requires more careful database constraint design.
- May require filtered unique index support depending on database provider.
- Can produce multiple historical rows for the same symbol.
- Adds unnecessary complexity for the current learning-focused MVP.

This option is not used in the current project.

---

## Current MVP Decision

For the current MVP, the project uses the simpler and more consistent rule:

    NormalizedSymbol should be unique across all records.

This means:

    One symbol should have one watchlist item record.

Current behavior:

| Existing Record State | POST Behavior | Status Code |
|---|---|---|
| No existing record | Create a new `WatchlistItem` | `201 Created` |
| Existing record is active | Return duplicate conflict | `409 Conflict` |
| Existing record is inactive | Reactivate existing `WatchlistItem` | `200 OK` |

This keeps the beginner MVP simple while also avoiding confusing API behavior.

A soft-deleted symbol should not be inserted as a new row.

A soft-deleted symbol should be reactivated by setting:

    IsActive = true
    UpdatedAtUtc = current UTC time

The original `CreatedAtUtc` should remain unchanged.

---

## Reactivation Standard

### Decision

If a client posts a symbol that already exists as an inactive `WatchlistItem`, the application should reactivate the existing record.

Reactivation means:

    IsActive = true
    UpdatedAtUtc = current UTC time

The application may also update these fields from the request:

    DisplayName
    Market

Reactivation should not change:

    Id
    CreatedAtUtc

Reactivation should not create:

    A new WatchlistItem row

Expected API result:

    200 OK

---

## Why Reactivation Is Needed

Without reactivation, the API can become confusing.

Example problematic behavior:

    GET /api/watchlist-items → []
    POST /api/watchlist-items with AAPL → 409 Conflict
    DELETE /api/watchlist-items/AAPL → 404 Not Found

From the client perspective, this feels inconsistent:

    The list says the symbol is not active.
    Create says the symbol already exists.
    Delete says the symbol is not found.

Reactivation solves this by defining a clear lifecycle:

    Create new symbol → active
    Delete symbol → inactive
    Post same symbol again → active again

This keeps one database row per symbol and makes the API easier to understand.

---

## Soft Delete and Reactivation Lifecycle

The WatchlistItem lifecycle is:

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

This lifecycle avoids duplicate rows while still allowing the user to add a previously deleted symbol again.

---

## Decimal Usage Standard

### Decision

Financial values should use:

    decimal

Do not use:

    double
    float

---

## Why Use Decimal for Financial Values

Financial values need precision.

`double` and `float` are binary floating-point types and may introduce precision issues.

For finance-related values such as prices, thresholds, and monetary values, `decimal` is more appropriate.

---

## Fields That Should Use Decimal

| Entity | Field | Type |
|---|---|---|
| `PriceSnapshot` | `Price` | `decimal` |
| `PriceAlert` | `TargetPrice` | `decimal` |

Future finance-related fields should also use `decimal`.

Examples:

    OpenPrice
    ClosePrice
    HighPrice
    LowPrice
    ChangeAmount
    ThresholdPrice

---

## Decimal Rule

Core rule:

    Use decimal for financial values.

Avoid:

    double Price
    float TargetPrice

Use:

    decimal Price
    decimal TargetPrice

---

## UTC Date/Time Standard

### Decision

Date/time fields should use UTC.

Field names should clearly indicate UTC usage by using the `Utc` suffix.

Examples:

    CreatedAtUtc
    UpdatedAtUtc
    RetrievedAtUtc
    LastTriggeredAtUtc
    CompletedAtUtc

---

## Why Use UTC

UTC avoids ambiguity across time zones.

This is important for:

- Logging
- Price snapshots
- Background worker processing
- External API data retrieval
- Future alert evaluation
- Future observability
- Debugging production-like issues

If local time is mixed with UTC, time-based behavior can become difficult to understand.

---

## Date Field Standard

| Entity | Field | Standard |
|---|---|---|
| `WatchlistItem` | `CreatedAtUtc` | UTC |
| `WatchlistItem` | `UpdatedAtUtc` | UTC or null |
| `PriceSnapshot` | `RetrievedAtUtc` | UTC |
| `PriceSnapshot` | `CreatedAtUtc` | UTC |
| `PriceAlert` | `CreatedAtUtc` | UTC |
| `PriceAlert` | `LastTriggeredAtUtc` | UTC or null |
| `ActionItem` | `CreatedAtUtc` | UTC |
| `ActionItem` | `CompletedAtUtc` | UTC or null |

---

## UTC Naming Rule

If a field stores UTC time, the name should end with:

    Utc

Preferred:

    CreatedAtUtc
    UpdatedAtUtc
    RetrievedAtUtc

Avoid:

    CreatedAt
    UpdatedAt
    SnapshotTime

Reason:

    The field name should make the time standard explicit.

---

## Date Creation Standard

When creating new records, use UTC time.

Recommended logic:

    DateTime.UtcNow

Example:

    CreatedAtUtc = DateTime.UtcNow

Avoid:

    DateTime.Now

Reason:

    DateTime.Now depends on local machine time zone.

---

## Date Update Standard

When updating, soft deleting, or reactivating an existing record, use UTC time.

Recommended logic:

    UpdatedAtUtc = DateTime.UtcNow

This applies to:

- Soft delete
- Reactivation
- Future update operations
- Future alert state changes
- Future action item completion

---

## Entity Response Exposure Standard

Entity models should not be returned directly from API endpoints.

Core rule:

    Entity = database persistence model
    DTO = API request/response contract model

API endpoints should return DTOs.

Example:

    WatchlistItem Entity
          ↓
    WatchlistItemResponse DTO
          ↓
    API Response

---

## Why Entities Should Not Be Returned Directly

Returning Entity models directly can create problems:

- Internal database fields may be exposed.
- Foreign keys may leak to API consumers unnecessarily.
- Navigation properties may create circular reference risks.
- Database model changes may break API response contracts.
- The API becomes tightly coupled to persistence design.

DTOs keep API responses controlled and stable.

---

## Validation Responsibility Standard

Validation should be separated by responsibility.

| Validation Type | Recommended Place | Example |
|---|---|---|
| Basic request validation | DTO / Controller model validation | Required `Symbol` |
| Business rule validation | Service | Active duplicate rule and inactive reactivation rule |
| Database consistency | Database constraint / EF Core configuration | Unique `NormalizedSymbol` |
| Data access existence check | Repository | `GetByNormalizedSymbolAsync` |

---

## Example Validation Flow

    Request DTO receives Symbol
          ↓
    Basic validation checks required fields
          ↓
    Service normalizes Symbol
          ↓
    Service asks Repository for existing NormalizedSymbol
          ↓
    Repository checks database
          ↓
    Service checks existing record state
          ↓
    Service decides whether to create, reject, or reactivate
          ↓
    Repository persists Entity changes

---

## Repository Query Standard

Repository queries should use normalized values when looking up symbols.

Current method:

    GetByNormalizedSymbolAsync

The implementation should compare against:

    NormalizedSymbol

Reason:

    The system should not depend on the casing or whitespace of user input.

Important distinction:

    GetAllActiveAsync should return only active records.
    GetByNormalizedSymbolAsync should be able to return active or inactive records.

Reason:

    The create flow needs to detect inactive records for reactivation.

---

## WatchlistItem Constraint Standard

| Field | Standard |
|---|---|
| `Id` | Primary key |
| `Symbol` | Required |
| `NormalizedSymbol` | Required |
| `NormalizedSymbol` | Unique |
| `DisplayName` | Optional |
| `Market` | Optional |
| `IsActive` | Required |
| `CreatedAtUtc` | Required |
| `UpdatedAtUtc` | Optional; used for soft delete and reactivation |

---

## PriceSnapshot Constraint Standard

| Field | Standard |
|---|---|
| `Id` | Primary key |
| `WatchlistItemId` | Required foreign key |
| `Symbol` | Required |
| `Price` | Required decimal |
| `Currency` | Required |
| `Source` | Required |
| `RetrievedAtUtc` | Required UTC |
| `CreatedAtUtc` | Required UTC |

A `PriceSnapshot` should belong to a `WatchlistItem`.

---

## PriceAlert Constraint Standard

| Field | Standard |
|---|---|
| `Id` | Primary key |
| `WatchlistItemId` | Required foreign key |
| `Symbol` | Required |
| `ConditionType` | Required |
| `TargetPrice` | Required decimal |
| `IsActive` | Required |
| `CreatedAtUtc` | Required UTC |
| `LastTriggeredAtUtc` | Optional UTC |

A `PriceAlert` should belong to a `WatchlistItem`.

---

## ActionItem Constraint Standard

| Field | Standard |
|---|---|
| `Id` | Primary key |
| `WatchlistItemId` | Required foreign key |
| `PriceAlertId` | Optional foreign key |
| `Symbol` | Required |
| `Title` | Required |
| `Description` | Optional |
| `Status` | Required |
| `CreatedAtUtc` | Required UTC |
| `CompletedAtUtc` | Optional UTC |

An `ActionItem` should always belong to a `WatchlistItem`.

An `ActionItem` may optionally belong to a `PriceAlert`.

---

## EF Core Configuration Notes

EF Core should support these standards through model configuration.

Current EF Core configuration includes:

- Unique index for `NormalizedSymbol`
- Required fields
- Maximum string lengths
- Relationship configuration
- Foreign key configuration

Additional configuration may be introduced gradually as the project grows.

The documented standards should continue to guide future CRUD, Repository Pattern, LINQ, Redis, RabbitMQ, and Background Worker implementation.

---

## Unique Index Standard

The unique index is:

    NormalizedSymbol

Purpose:

    Prevent duplicate symbol records.

Example EF Core configuration:

    HasIndex(x => x.NormalizedSymbol).IsUnique()

This has been applied through EF Core model configuration and migration.

The unique index remains compatible with soft delete because the application reactivates inactive records instead of inserting duplicate rows.

---

## Maximum Length Standard

String fields should have clear length limits.

Example recommended limits:

| Field | Suggested Maximum Length |
|---|---|
| `Symbol` | 20 |
| `NormalizedSymbol` | 20 |
| `DisplayName` | 100 |
| `Market` | 50 |
| `Currency` | 10 |
| `Source` | 50 |
| `ConditionType` | 20 |
| `Status` | 30 |
| `Title` | 150 |
| `Description` | 500 |

These values can be adjusted later if the domain requires it.

The important standard is:

    String fields should not be unlimited by default.

---

## Database File Tracking Standard

The SQLite database file is a local runtime artifact.

It should not be committed to Git.

Do not commit:

    marketinsight.db
    *.db
    *.db-shm
    *.db-wal

Commit:

    Migration files
    AppDbContext
    Entity models
    Configuration files
    Documentation files

---

## Why SQLite Database Files Are Not Committed

SQLite database files should not be committed because:

- They are local runtime files.
- They may contain local development data.
- They may differ between machines.
- They can create unnecessary Git noise.
- The schema is already represented by migration files.

---

## Why Migration Files Are Committed

Migration files should be committed because:

- They represent database schema history.
- They allow the database to be recreated.
- They keep schema changes versioned.
- They help other developers apply the same database structure.
- They support future migration-based development.

---

## Git Ignore Standard

The project should ignore SQLite runtime files.

Required `.gitignore` rules:

    *.db
    *.db-shm
    *.db-wal

This keeps local SQLite database artifacts out of version control.

---

## API and Database Rule Alignment

Some rules should exist both in the API layer and database layer.

Example:

    Symbol uniqueness

API responsibility:

    Normalize symbol.
    Check existing symbol.
    Return conflict when the existing symbol is active.
    Reactivate the existing row when the existing symbol is inactive.

Database responsibility:

    Enforce unique normalized symbol constraint.

Reason:

    API validation provides a good user experience.
    Database constraints protect data integrity.

---

## Current MVP Decisions

| Decision Area | Current Decision |
|---|---|
| Duplicate symbols | Active duplicate symbols are not allowed |
| Symbol comparison | Use normalized symbol |
| Symbol normalization | Trim + uppercase |
| Soft delete | Use `IsActive = false` |
| Reactivation | POST same inactive symbol reactivates existing row |
| Unique symbol storage | One row per `NormalizedSymbol` |
| Financial values | Use decimal |
| Date/time values | Use UTC |
| Entity responses | Use DTOs, not Entities |
| SQLite database file | Do not commit |
| Migration files | Commit |
| Watchlist delete behavior | Soft delete through `IsActive` |

---

## Review Checklist

Before implementing or modifying Watchlist CRUD, check:

- Is `Symbol` required?
- Is `NormalizedSymbol` required?
- Is `NormalizedSymbol` used for duplicate checks?
- Is `NormalizedSymbol` unique across all records?
- Is the active duplicate symbol rule clear?
- Is the inactive reactivation rule clear?
- Is symbol normalization defined as trim + uppercase?
- Are financial values using `decimal`?
- Are date/time fields using UTC?
- Do UTC fields use the `Utc` suffix?
- Is `UpdatedAtUtc` updated during soft delete?
- Is `UpdatedAtUtc` updated during reactivation?
- Are Entity models not returned directly from API endpoints?
- Are DTOs used for request and response contracts?
- Are migration files committed?
- Is `marketinsight.db` ignored by Git?
- Are SQLite runtime files ignored by Git?
- Are entity constraints documented before CRUD implementation?

---

## Future Implementation Notes

When implementing Repository Pattern and Watchlist CRUD, the following should be checked in code:

- Verify unique index on `NormalizedSymbol`.
- Normalize symbol before save and lookup.
- Reject active duplicate symbols at API/service level.
- Reactivate inactive existing symbols instead of inserting duplicate rows.
- Use DTOs for API request and response models.
- Keep Entity models inside persistence flow.
- Use `decimal` for price-related values.
- Use `DateTime.UtcNow` for created/updated timestamps.
- Update `UpdatedAtUtc` during soft delete and reactivation.
- Keep SQLite database files out of Git.

Future features should continue to respect these standards:

- Price refresh should use active watchlist items.
- Redis cache keys should use normalized symbols.
- RabbitMQ messages should use normalized symbols.
- Background Worker processing should validate that the watchlist item is active before processing.
- Price snapshots should reference the correct `WatchlistItemId`.

---

## Summary

The project uses clear entity constraint standards to keep data consistent.

Core rules:

- `Symbol` must be normalized.
- `NormalizedSymbol` should be unique.
- One normalized symbol should map to one database row.
- Active duplicate watchlist symbols are not allowed.
- Inactive existing symbols should be reactivated instead of inserted as new rows.
- Soft delete should use `IsActive = false`.
- Reactivation should use `IsActive = true`.
- `UpdatedAtUtc` should be updated during soft delete and reactivation.
- Financial values should use `decimal`.
- Date/time fields should use UTC.
- UTC fields should use the `Utc` suffix.
- Entity models should not be returned directly as API responses.
- Migration files should be committed.
- Local SQLite database files should not be committed.

These standards should guide Repository Pattern, LINQ, Watchlist CRUD, Redis, RabbitMQ, Background Worker, and future backend implementation.