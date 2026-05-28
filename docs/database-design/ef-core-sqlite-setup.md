# EF Core and SQLite Database Setup

## Purpose

This document describes the Entity Framework Core and SQLite database setup used in the MarketInsight Operations Tracker API.

The purpose of this setup is to connect the application to a persistent local database and prepare the database foundation for future repository, query, and CRUD operations.

This document covers:

    EF Core packages
    AppDbContext
    DbSet definitions
    SQLite connection string
    Program.cs database registration
    InitialCreate migration
    AddWatchlistItemConstraints migration
    SQLite database creation
    Git ignore rules for local database files

---

## Setup Context

The project uses SQLite as the persistent database.

Entity Framework Core is used as the ORM layer between the C# entity models and the SQLite database.

The project keeps the following technical responsibility separation:

    SQLite = persistent data
    Redis = cache
    RabbitMQ = async queue

SQLite stores application data that should remain available after the application stops.

Redis will be used later for short-term quote caching.

RabbitMQ will be used later for asynchronous price refresh messaging.

---

## Implemented Setup

The following setup was completed:

| Item | Status |
|---|---|
| EF Core packages added | Completed |
| `Data` folder created | Completed |
| `AppDbContext` created | Completed |
| DbSet definitions added | Completed |
| SQLite connection string configured | Completed |
| `AppDbContext` registered in `Program.cs` | Completed |
| `InitialCreate` migration created | Completed |
| SQLite database file created | Completed |
| SQLite database files added to `.gitignore` | Completed |
| Entity constraints configured in `AppDbContext` | Completed |
| `NormalizedSymbol` unique index configured | Completed |
| `AddWatchlistItemConstraints` migration created | Completed |
| Project build verified | Completed |

---

## EF Core Packages

The following NuGet packages were added to the API project:

| Package | Purpose |
|---|---|
| `Microsoft.EntityFrameworkCore` | Provides the main EF Core infrastructure |
| `Microsoft.EntityFrameworkCore.Sqlite` | Enables EF Core to use SQLite as the database provider |
| `Microsoft.EntityFrameworkCore.Design` | Enables design-time EF Core operations such as migrations |

These packages allow the project to:

- Define database models with C# entity classes
- Configure a database context
- Generate EF Core migrations
- Apply migrations to a SQLite database
- Keep database schema changes versioned through migration files

---

## AppDbContext

`AppDbContext` is the main EF Core database context of the application.

It represents the connection point between the application entity models and the SQLite database.

Implementation location:

    src/MarketInsight.Api/Data/AppDbContext.cs

The context inherits from:

    DbContext

It receives database configuration through:

    DbContextOptions<AppDbContext>

This allows the application to configure SQLite as the database provider in `Program.cs`.

---

## DbSet Definitions

The following DbSet definitions were added to `AppDbContext`:

| DbSet | Entity | Purpose |
|---|---|---|
| `WatchlistItems` | `WatchlistItem` | Stores financial symbols tracked by the user |
| `PriceSnapshots` | `PriceSnapshot` | Stores historical price records for tracked symbols |
| `PriceAlerts` | `PriceAlert` | Stores price alert rules for tracked symbols |
| `ActionItems` | `ActionItem` | Stores follow-up actions created after price alerts or important price events |

Application database context structure:

    AppDbContext
      ├── WatchlistItems
      ├── PriceSnapshots
      ├── PriceAlerts
      └── ActionItems

`PriceAlert` and `ActionItem` are included in the database context because they are part of the project scope.

At this stage, only their database foundation is prepared.

Their business logic, evaluation flow, and endpoint implementation are outside the current scope.

---

## Entity Configuration

Entity constraints, indexes, and relationships are configured in:

    src/MarketInsight.Api/Data/AppDbContext.cs

The project uses `OnModelCreating` to define database-level rules.

This keeps important persistence rules explicit and versioned through EF Core migrations.

Configured areas include:

- Primary keys
- Required fields
- Maximum string lengths
- Entity relationships
- Foreign keys
- Delete behavior
- Unique index for `NormalizedSymbol`

---

## WatchlistItem Configuration

`WatchlistItem` stores financial symbols tracked by the system.

Configured standards:

| Field | Configuration |
|---|---|
| `Id` | Primary key |
| `Symbol` | Required, maximum length 20 |
| `NormalizedSymbol` | Required, maximum length 20, unique index |
| `DisplayName` | Maximum length 100 |
| `Market` | Maximum length 50 |

The most important database constraint for `WatchlistItem` is:

    NormalizedSymbol must be unique.

This supports the rule that the same financial symbol should not be added to the watchlist more than once.

Example:

    aapl
    AAPL
    " AAPL "

All should be normalized and treated as:

    AAPL

---

## PriceSnapshot Configuration

`PriceSnapshot` stores historical price records for tracked financial symbols.

Configured standards:

| Field | Configuration |
|---|---|
| `Id` | Primary key |
| `WatchlistItemId` | Required foreign key |
| `Symbol` | Required, maximum length 20 |
| `Price` | Required decimal value |
| `Currency` | Required, maximum length 10 |
| `Source` | Required, maximum length 50 |

Relationship:

    WatchlistItem 1 - N PriceSnapshot

Delete behavior:

    Cascade delete from WatchlistItem to related PriceSnapshot records.

---

## PriceAlert Configuration

`PriceAlert` stores price alert rules for tracked financial symbols.

Configured standards:

| Field | Configuration |
|---|---|
| `Id` | Primary key |
| `WatchlistItemId` | Required foreign key |
| `Symbol` | Required, maximum length 20 |
| `ConditionType` | Required, maximum length 20 |
| `TargetPrice` | Required decimal value |

Relationship:

    WatchlistItem 1 - N PriceAlert

Delete behavior:

    Cascade delete from WatchlistItem to related PriceAlert records.

---

## ActionItem Configuration

`ActionItem` stores operational follow-up actions.

Configured standards:

| Field | Configuration |
|---|---|
| `Id` | Primary key |
| `WatchlistItemId` | Required foreign key |
| `PriceAlertId` | Optional foreign key |
| `Symbol` | Required, maximum length 20 |
| `Title` | Required, maximum length 150 |
| `Description` | Optional, maximum length 500 |
| `Status` | Required, maximum length 30 |

Relationships:

    WatchlistItem 1 - N ActionItem
    PriceAlert 1 - N ActionItem

`PriceAlertId` is optional because an `ActionItem` may come from a price alert or from another important price event in the future.

Delete behavior:

    Cascade delete from WatchlistItem to related ActionItem records.
    Set PriceAlertId to null if the related PriceAlert is deleted.

---

## SQLite Connection String

The SQLite connection string was added to:

    src/MarketInsight.Api/appsettings.json

Configured section:

    ConnectionStrings

Configured key:

    DefaultConnection

Configured value:

    Data Source=marketinsight.db

This configuration tells EF Core to use a local SQLite database file named:

    marketinsight.db

The database file is generated when migrations are applied.

---

## Program.cs Registration

`AppDbContext` was registered in the dependency injection container.

Implementation location:

    src/MarketInsight.Api/Program.cs

Required namespaces:

    using MarketInsight.Api.Data;
    using Microsoft.EntityFrameworkCore;

Database registration:

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

This registration allows the application to create and use `AppDbContext` with the configured SQLite connection string.

---

## Initial Migration

The first EF Core migration was created with the name:

    InitialCreate

Migration location:

    src/MarketInsight.Api/Migrations

Generated migration files include:

| File | Purpose |
|---|---|
| `*_InitialCreate.cs` | Contains the database schema operations for the initial migration |
| `*_InitialCreate.Designer.cs` | Contains EF Core generated migration metadata |
| `AppDbContextModelSnapshot.cs` | Stores the current EF Core model snapshot |

Migration files should be committed to Git because they describe the database schema.

The `InitialCreate` migration prepares the initial database structure for the entity model foundation.

---

## Constraint Migration

After documenting the entity constraint standards, an additional migration was created to align the database model with the documented standards.

Migration name:

    AddWatchlistItemConstraints

Migration location:

    src/MarketInsight.Api/Migrations

This migration adds the unique index for:

    WatchlistItem.NormalizedSymbol

Expected index name:

    IX_WatchlistItems_NormalizedSymbol

Purpose:

    Prevent duplicate normalized financial symbols in the watchlist.

This supports the documented rule:

    NormalizedSymbol should be unique.

---

## Database Update

The migrations were applied to the SQLite database.

Command used:

    dotnet ef database update

The database update completed successfully.

Result:

    marketinsight.db

The SQLite database file was created locally under the API project folder.

Database file location:

    src/MarketInsight.Api/marketinsight.db

---

## SQLite Database File and Git Ignore Rule

The SQLite database file is a local runtime artifact.

It should not be committed to Git.

The following rules were added to `.gitignore`:

    *.db
    *.db-shm
    *.db-wal

This keeps local SQLite database files out of version control.

Correct Git behavior:

    Migration files are committed.
    SQLite database files are not committed.

---

## Current Database Tables

The migrations prepare the database structure for the current entity model foundation.

Expected application tables:

| Table | Source Entity | Purpose |
|---|---|---|
| `WatchlistItems` | `WatchlistItem` | Stores tracked financial symbols |
| `PriceSnapshots` | `PriceSnapshot` | Stores historical price snapshot records |
| `PriceAlerts` | `PriceAlert` | Stores price alert rules |
| `ActionItems` | `ActionItem` | Stores operational follow-up actions |

EF Core also creates an internal migration history table:

| Table | Purpose |
|---|---|
| `__EFMigrationsHistory` | Stores applied EF Core migration records |

---

## Current Database Constraints

Detailed entity constraint decisions are documented in:

    docs/database-design/entity-constraint-standards.md

This section only describes the constraints currently configured through EF Core.

The current database model includes the following important constraints:

| Area | Constraint |
|---|---|
| `WatchlistItem.Symbol` | Required, maximum length 20 |
| `WatchlistItem.NormalizedSymbol` | Required, maximum length 20, unique index |
| `PriceSnapshot.Symbol` | Required, maximum length 20 |
| `PriceSnapshot.Price` | Required decimal value |
| `PriceSnapshot.Currency` | Required, maximum length 10 |
| `PriceSnapshot.Source` | Required, maximum length 50 |
| `PriceAlert.Symbol` | Required, maximum length 20 |
| `PriceAlert.ConditionType` | Required, maximum length 20 |
| `PriceAlert.TargetPrice` | Required decimal value |
| `ActionItem.Symbol` | Required, maximum length 20 |
| `ActionItem.Title` | Required, maximum length 150 |
| `ActionItem.Status` | Required, maximum length 30 |

---

## Current Scope

This document covers the SQLite and EF Core setup completed for the database foundation.

The following items are included in the current scope:

- EF Core package installation
- SQLite provider setup
- `AppDbContext` creation
- DbSet definitions
- SQLite connection string configuration
- Dependency injection registration for `AppDbContext`
- Initial EF Core migration
- Entity constraint configuration
- `NormalizedSymbol` unique index configuration
- Constraint migration
- SQLite database creation
- `.gitignore` update for local SQLite database files
- Build verification

---

## Out of Scope

The following topics are outside the scope of this document and will be documented separately as they are implemented:

- Repository Pattern implementation
- Watchlist CRUD endpoints
- Controller-to-service flow
- Service layer implementation
- Watchlist validation logic
- Duplicate symbol control at service level
- Symbol normalization implementation at service level
- Price refresh endpoint
- PriceSnapshot creation during refresh
- External Finance API integration
- Redis cache implementation
- RabbitMQ messaging implementation
- Background Worker implementation
- PriceAlert business logic
- Alert evaluation logic
- ActionItem workflow
- ActionItem endpoint implementation

---

## Verification

The following verification steps were completed.

### EF Core CLI Verification

EF Core command-line tools were verified.

Result:

    Entity Framework Core .NET Command-line Tools
    8.0.27

### Initial Migration Verification

The `InitialCreate` migration was created successfully.

Migration folder:

    src/MarketInsight.Api/Migrations

### Constraint Migration Verification

The `AddWatchlistItemConstraints` migration was created successfully.

The migration adds a unique index for:

    NormalizedSymbol

Expected index:

    IX_WatchlistItems_NormalizedSymbol

### Database Verification

The migrations were applied successfully.

The SQLite database file was created:

    src/MarketInsight.Api/marketinsight.db

### Build Verification

After configuring EF Core, SQLite, migrations, database update, and entity constraints, the project build was executed.

Build command:

    dotnet build

Result:

    Build succeeded

---

## Related Files

| File | Purpose |
|---|---|
| `src/MarketInsight.Api/Data/AppDbContext.cs` | EF Core database context and entity configuration |
| `src/MarketInsight.Api/appsettings.json` | SQLite connection string configuration |
| `src/MarketInsight.Api/Program.cs` | AppDbContext dependency injection registration |
| `src/MarketInsight.Api/MarketInsight.Api.csproj` | EF Core package references |
| `src/MarketInsight.Api/Migrations/*_InitialCreate.cs` | Initial EF Core migration |
| `src/MarketInsight.Api/Migrations/*_AddWatchlistItemConstraints.cs` | EF Core migration for entity constraints and unique index |
| `src/MarketInsight.Api/Migrations/AppDbContextModelSnapshot.cs` | EF Core model snapshot |
| `.gitignore` | Ignores local SQLite database files |
| `docs/database-design/ef-core-sqlite-setup.md` | Documents the EF Core and SQLite setup |
| `docs/database-design/entity-constraint-standards.md` | Documents detailed entity constraint decisions |

---

## Status

SQLite is configured as the persistent database for the project.

Entity Framework Core is connected to SQLite through `AppDbContext`.

The initial migration was created and applied successfully.

The entity constraint migration was created and applied successfully.

The local SQLite database file was generated and excluded from Git tracking.

The project builds successfully.

The database foundation is ready for the next implementation step:

    Repository Pattern and LINQ