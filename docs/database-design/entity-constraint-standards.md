# Entity Constraint Standards

This document defines the entity-level constraint and data consistency standards for the MarketInsight Operations Tracker API.

The purpose of this document is to clarify important database and validation decisions before implementing Repository Pattern, LINQ queries, and Watchlist CRUD endpoints.

---

## Purpose

Entity constraint standards help the project keep data consistent, predictable, and easy to maintain.

This document defines standards for:

- Symbol uniqueness
- Symbol normalization
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
- Financial values may lose precision if `double` or `float` is used.
- Date values may become inconsistent if local time and UTC are mixed.
- API validation may allow data that the database should reject.
- Database rules and application rules may conflict.

This document prevents these problems by defining clear standards.

---

## Constraint Summary

| Area | Standard |
|---|---|
| Symbol uniqueness | One normalized symbol should exist only once |
| Symbol normalization | Trim whitespace and convert to uppercase |
| Financial values | Use `decimal` |
| Date/time values | Use UTC and name fields with `Utc` suffix |
| Entity response exposure | Do not return Entity models directly from API endpoints |
| Database file tracking | Do not commit local SQLite database files |
| Migration tracking | Commit migration files |

---

## Symbol Uniqueness Standard

### Decision

The same financial symbol should not be added to the watchlist more than once.

The project should treat these values as the same symbol:

    aapl
    Aapl
    AAPL
    " AAPL "

After normalization, all of them become:

    AAPL

Therefore, only one `WatchlistItem` should exist for:

    AAPL

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

---

## Standard Rule

The project should enforce symbol uniqueness using the normalized symbol value.

Main rule:

    NormalizedSymbol must be unique.

This means the system should use `NormalizedSymbol` for lookup and duplicate checks.

---

## Symbol Fields

| Field | Purpose |
|---|---|
| `Symbol` | Stores the original or display symbol value |
| `NormalizedSymbol` | Stores the normalized value used for lookup, comparison, and duplicate control |

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
| Service | Normalizes the symbol |
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
    Service checks duplicate rule using repository
          ↓
    Repository queries by NormalizedSymbol
          ↓
    Entity is saved with normalized value

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

This means deletion may be implemented as soft delete:

    IsActive = false

There are two possible uniqueness approaches.

---

## Option 1: Unique NormalizedSymbol Across All Records

Rule:

    A symbol can exist only once in the table, even if inactive.

Pros:

- Simple.
- Easy to understand.
- Prevents historical duplicate records.

Cons:

- Re-adding a deleted symbol requires reactivating the old record instead of inserting a new one.

---

## Option 2: Unique Active NormalizedSymbol

Rule:

    A symbol can exist only once while active.

Pros:

- Allows historical inactive records.
- Allows re-adding a symbol after deletion.

Cons:

- Requires more careful database constraint design.
- May require filtered unique index support depending on database provider.

---

## Current MVP Decision

For the current MVP, the project should use the simpler rule:

    NormalizedSymbol should be unique across all records.

This means:

    One symbol should have one watchlist item record.

If the user deletes a symbol and later adds it again, the application may reactivate the existing record in a future implementation.

This keeps the beginner MVP simpler and avoids unnecessary complexity.

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
| Business rule validation | Service | Duplicate symbol rule |
| Database consistency | Database constraint / EF Core configuration | Unique `NormalizedSymbol` |
| Data access existence check | Repository | `ExistsBySymbolAsync` |

---

## Example Validation Flow

    Request DTO receives Symbol
          ↓
    Basic validation checks required fields
          ↓
    Service normalizes Symbol
          ↓
    Service asks Repository if NormalizedSymbol exists
          ↓
    Repository checks database
          ↓
    Service decides whether creation is allowed
          ↓
    Repository persists Entity

---

## Repository Query Standard

Repository queries should use normalized values when looking up symbols.

Example future method:

    GetByNormalizedSymbolAsync

or:

    GetBySymbolAsync

The implementation should compare against:

    NormalizedSymbol

Reason:

    The system should not depend on the casing or whitespace of user input.

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
| `UpdatedAtUtc` | Optional |

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

The documented standards should continue to guide future CRUD, Repository Pattern, and LINQ implementation.

---

## Unique Index Standard

The unique index is:

    NormalizedSymbol

Purpose:

    Prevent duplicate symbol records.

Example EF Core configuration:

    HasIndex(x => x.NormalizedSymbol).IsUnique()

This has been applied through EF Core model configuration and migration.

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

    Check duplicate symbol before creating a new item.

Database responsibility:

    Enforce unique normalized symbol constraint.

Reason:

    API validation provides a good user experience.
    Database constraints protect data integrity.

---

## Current MVP Decisions

| Decision Area | Current Decision |
|---|---|
| Duplicate symbols | Not allowed |
| Symbol comparison | Use normalized symbol |
| Symbol normalization | Trim + uppercase |
| Financial values | Use decimal |
| Date/time values | Use UTC |
| Entity responses | Use DTOs, not Entities |
| SQLite database file | Do not commit |
| Migration files | Commit |
| Watchlist delete behavior | Soft delete can be used through `IsActive` |

---

## Review Checklist

Before implementing Watchlist CRUD, check:

- Is `Symbol` required?
- Is `NormalizedSymbol` required?
- Is `NormalizedSymbol` used for duplicate checks?
- Is the duplicate symbol rule clear?
- Is symbol normalization defined as trim + uppercase?
- Are financial values using `decimal`?
- Are date/time fields using UTC?
- Do UTC fields use the `Utc` suffix?
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
- Reject duplicate symbols at API/service level.
- Use DTOs for API request and response models.
- Keep Entity models inside persistence flow.
- Use `decimal` for price-related values.
- Use `DateTime.UtcNow` for created/updated timestamps.
- Keep SQLite database files out of Git.

---

## Summary

The project uses clear entity constraint standards to keep data consistent.

Core rules:

- `Symbol` must be normalized.
- `NormalizedSymbol` should be unique.
- Duplicate watchlist symbols are not allowed.
- Financial values should use `decimal`.
- Date/time fields should use UTC.
- UTC fields should use the `Utc` suffix.
- Entity models should not be returned directly as API responses.
- Migration files should be committed.
- Local SQLite database files should not be committed.

These standards should guide Repository Pattern, LINQ, and Watchlist CRUD implementation.