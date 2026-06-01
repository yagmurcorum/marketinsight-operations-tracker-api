# Layer Responsibility Standard

This document defines the responsibility boundaries between the main application layers of the MarketInsight Operations Tracker API.

The purpose of this standard is to keep the project understandable, maintainable, and consistent while implementing and maintaining Repository Pattern, LINQ queries, Watchlist CRUD endpoints, soft delete behavior, and reactivation behavior.

---

## Architecture Style

The project follows a learning-focused layered monolith architecture.

This means:

- The project is developed as a single backend API application.
- The code is separated into clear responsibility layers.
- The goal is not to build a distributed microservice system.
- The goal is to learn how backend responsibilities are separated in a professional API project.

Main layers:

- Controller
- Service
- Repository
- Entity
- DTO

---

## Why Layer Responsibility Matters

Without clear layer responsibility, code can become difficult to understand and maintain.

For example:

- Controller may start containing business rules.
- Repository may start deciding business behavior.
- Entity may be returned directly from the API.
- DTO and Entity may become mixed.
- Validation and database access may become scattered.
- Soft delete behavior may become inconsistent.
- Reactivation behavior may be implemented in the wrong layer.

This project avoids that by defining clear boundaries.

---

## High-Level Layer Flow

    Client / Swagger
          ↓
    Controller
          ↓
    Service
          ↓
    Repository
          ↓
    AppDbContext / EF Core
          ↓
    SQLite Database

Response flow:

    SQLite Database
          ↓
    Entity
          ↓
    Repository
          ↓
    Service
          ↓
    DTO
          ↓
    Controller
          ↓
    Client / Swagger

---

## Layer Summary

| Layer | Main Responsibility | Should Not Do |
|---|---|---|
| Controller | Handles HTTP request and response flow | Should not contain business logic |
| Service | Handles use cases and business rules | Should not directly expose database implementation details |
| Repository | Handles database access | Should not contain business decision logic |
| Entity | Represents database persistence model | Should not be used as direct API response |
| DTO | Represents API request and response contract | Should not represent database relationships |

---

## Controller Layer

### Purpose

The Controller layer handles HTTP communication.

It receives requests from the client, calls the required application logic, and returns HTTP responses.

In this project, controllers are responsible for API endpoint behavior.

Current controller example:

    WatchlistItemsController

Current routes:

    GET /api/watchlist-items
    GET /api/watchlist-items/{symbol}
    POST /api/watchlist-items
    DELETE /api/watchlist-items/{symbol}

---

### Controller Responsibilities

Controllers should:

- Define API routes.
- Receive request DTOs.
- Validate basic model state when needed.
- Call the relevant service method.
- Return proper HTTP status codes.
- Return response DTOs.
- Keep endpoint behavior readable.
- Use XML Summary comments for Swagger documentation.

For Watchlist Items, the Controller maps Service results to HTTP responses.

Example:

| Service Result | HTTP Response |
|---|---|
| Created | `201 Created` |
| Reactivated | `200 OK` |
| Duplicate | `409 Conflict` |
| Not found | `404 Not Found` |
| Deleted | `204 No Content` |

---

### Controller Should Not

Controllers should not:

- Contain business logic.
- Directly use `AppDbContext`.
- Directly write EF Core queries.
- Decide database persistence rules.
- Perform duplicate symbol checks by itself.
- Decide whether inactive records should be reactivated.
- Perform complex validation rules by itself.
- Return Entity models directly.
- Contain long procedural workflows.

---

### Controller Example Responsibility

Correct controller mindset:

    "I receive the HTTP request, pass it to the service, and return the correct HTTP response."

Wrong controller mindset:

    "I decide all business rules and directly query the database."

---

## Service Layer

### Purpose

The Service layer handles use cases and business rules.

It coordinates the application behavior between Controller and Repository.

In this project, services are responsible for business decisions such as:

- Symbol normalization
- Active duplicate symbol validation
- Watchlist item creation logic
- Inactive watchlist item reactivation
- Soft delete behavior
- Mapping between Entity and DTO
- Future alert evaluation flow
- Future price refresh coordination

---

### Service Responsibilities

Services should:

- Implement use case logic.
- Apply business rules.
- Coordinate repository calls.
- Normalize input values.
- Validate business conditions.
- Decide what should happen when data exists or does not exist.
- Decide what should happen when existing data is active or inactive.
- Map request DTOs to Entity models.
- Map Entity models to response DTOs.
- Return application-level results to controllers.

For Watchlist Items, the Service owns these business rules:

    New symbol → create new record
    Existing active symbol → return duplicate result
    Existing inactive symbol → reactivate existing record
    Delete active symbol → set IsActive to false

---

### Service Should Not

Services should not:

- Define HTTP routes.
- Return raw database queries to controllers.
- Contain direct SQL.
- Depend on Swagger or HTTP-specific metadata.
- Be responsible for database schema configuration.
- Be responsible for EF Core migration logic.
- Return HTTP-specific response objects such as `Ok`, `Created`, or `Conflict`.

---

### Service Example Responsibility

Example Watchlist create flow:

    CreateWatchlistItemRequest DTO
          ↓
    Validate basic input through model validation
          ↓
    Normalize symbol
          ↓
    Ask Repository for existing record by NormalizedSymbol
          ↓
    If no existing record:
          Create WatchlistItem Entity
          Save with Repository
          Return Created result
          ↓
    If existing record is active:
          Return Duplicate result
          ↓
    If existing record is inactive:
          Reactivate existing WatchlistItem Entity
          Update UpdatedAtUtc
          Save with Repository
          Return Reactivated result
          ↓
    Controller maps result to HTTP response

Correct service mindset:

    "I decide what the application should do."

Wrong service mindset:

    "I only pass data from controller to repository without owning any use case rules."

---

## Repository Layer

### Purpose

The Repository layer handles database access.

It isolates EF Core and data access logic from the rest of the application.

In this project, repositories use `AppDbContext` and `DbSet` objects to query and persist data.

Current repository example:

    IWatchlistItemRepository
    WatchlistItemRepository

---

### Repository Responsibilities

Repositories should:

- Read data from the database.
- Add new records to the database.
- Update existing tracked records through EF Core change tracking.
- Apply simple data access filters.
- Use EF Core LINQ queries.
- Use async database operations.
- Access `AppDbContext`.
- Work with Entity models.
- Hide EF Core details from services.

---

### Repository Should Not

Repositories should not:

- Define HTTP routes.
- Return HTTP status codes.
- Use request or response DTOs as primary models.
- Contain business rule decisions.
- Decide whether an operation is allowed from a business perspective.
- Normalize symbols as a business rule.
- Decide whether an inactive item should be reactivated.
- Decide API response shape.
- Know about Swagger documentation.
- Contain UI or client-specific logic.

---

### Repository Example Responsibility

Current methods:

    GetAllActiveAsync
    GetByNormalizedSymbolAsync
    ExistsByNormalizedSymbolAsync
    AddAsync
    SaveChangesAsync

Correct repository mindset:

    "I know how to access and persist data."

Wrong repository mindset:

    "I decide the business meaning of the operation."

Important distinction:

    GetAllActiveAsync returns only active records.
    GetByNormalizedSymbolAsync may return active or inactive records.

Reason:

    The Service needs to inspect whether an existing WatchlistItem is active or inactive before deciding create, conflict, or reactivation behavior.

---

## Entity Layer

### Purpose

Entity models represent database persistence models.

They describe how data is stored in the database.

In this project, Entity models are stored under:

    src/MarketInsight.Api/Entities

Current entities:

- WatchlistItem
- PriceSnapshot
- PriceAlert
- ActionItem

---

### Entity Responsibilities

Entities should:

- Represent database records.
- Define persistence fields.
- Contain primary key fields.
- Contain foreign key fields when needed.
- Represent relationships between database models.
- Use correct data types for persistence.
- Support EF Core mapping and migration generation.

---

### Entity Should Not

Entities should not:

- Be returned directly as API responses.
- Be used as request body models.
- Contain API-specific response shaping.
- Contain Swagger-specific documentation behavior.
- Contain business workflow logic.
- Contain controller or service dependencies.
- Expose internal database structure directly to API consumers.

---

### Entity Example

`WatchlistItem` represents a financial symbol tracked by the system.

It is a database model.

It should not be treated as the public API response contract.

Correct entity mindset:

    "I represent how data is stored."

Wrong entity mindset:

    "I am the object that the API should directly return to clients."

---

## DTO Layer

### Purpose

DTO means Data Transfer Object.

DTO models represent API request and response contracts.

They define how data enters and leaves the API.

In this project, DTO models are stored under:

    src/MarketInsight.Api/DTOs

Current Watchlist DTOs:

- CreateWatchlistItemRequest
- WatchlistItemResponse

---

### DTO Responsibilities

DTOs should:

- Represent API request bodies.
- Represent API response bodies.
- Define public API contract shape.
- Hide unnecessary internal database fields.
- Keep API responses stable.
- Support Swagger documentation.
- Separate external API contract from internal database model.

---

### DTO Should Not

DTOs should not:

- Represent database schema directly.
- Contain navigation properties.
- Contain EF Core configuration.
- Be used as database persistence models.
- Contain business logic.
- Replace Entity models.
- Depend on database relationships.

---

### DTO Example

`CreateWatchlistItemRequest` represents the request body for creating or reactivating a watchlist item.

`WatchlistItemResponse` represents the response returned from the API.

Correct DTO mindset:

    "I represent what the API receives or returns."

Wrong DTO mindset:

    "I am the database model."

---

## Entity and DTO Separation Standard

The project must keep Entity and DTO models separate.

Core rule:

    Entity = database / persistence model
    DTO = API request / response contract model

---

## Why Entities Should Not Be Returned Directly

Entity models should not be returned directly from API endpoints because:

- They may expose internal database fields.
- They may expose foreign keys or navigation properties.
- They may create circular reference risks.
- They couple the API contract to the database model.
- Database changes may accidentally break API consumers.
- Public response shape becomes harder to control.

Instead, controllers should return DTO models.

Example:

    WatchlistItem Entity
          ↓
    WatchlistItemResponse DTO
          ↓
    API Response

---

## Request and Response Model Standard

The project should use separate DTOs for request and response models when needed.

Example:

    CreateWatchlistItemRequest
    WatchlistItemResponse

Reason:

- Request models define what the API accepts.
- Response models define what the API returns.
- The two shapes do not have to be identical.

For example, when creating or reactivating a watchlist item, the client may only send:

    symbol
    displayName
    market

But the response may include:

    id
    symbol
    normalizedSymbol
    displayName
    market
    isActive
    createdAtUtc
    updatedAtUtc

---

## Business Logic Placement

Business logic should be placed in the Service layer.

Examples of business logic:

- Symbol normalization
- Active duplicate symbol check decision
- Whether a watchlist item can be created
- Whether an inactive watchlist item should be reactivated
- Whether a watchlist item can be deleted
- Whether soft delete should be used
- Future alert evaluation rules
- Future action item creation rules

---

## Data Access Placement

Data access should be placed in the Repository layer.

Examples of data access:

- Query all active watchlist items
- Find watchlist item by normalized symbol
- Check whether symbol already exists
- Add new watchlist item
- Update existing entity through EF Core tracking
- Save changes through EF Core

---

## HTTP Responsibility Placement

HTTP-specific responsibility should stay in the Controller layer.

Examples of HTTP responsibility:

- Route definition
- HTTP method selection
- Status code selection
- Returning `Ok`
- Returning `Created`
- Returning `Conflict`
- Returning `NotFound`
- Returning `BadRequest`
- Returning `NoContent`

---

## Example Watchlist Create Flow

    Client sends POST /api/watchlist-items
          ↓
    Controller receives CreateWatchlistItemRequest
          ↓
    Controller calls Service
          ↓
    Service normalizes Symbol
          ↓
    Service asks Repository for existing record by NormalizedSymbol
          ↓
    Repository returns existing Entity or null
          ↓
    Service decides business result:
          - No existing record → create new WatchlistItem
          - Existing active record → duplicate result
          - Existing inactive record → reactivate existing WatchlistItem
          ↓
    Repository saves Entity changes through AppDbContext
          ↓
    Service maps Entity to WatchlistItemResponse
          ↓
    Controller returns HTTP response:
          - Created result → 201 Created
          - Reactivated result → 200 OK
          - Duplicate result → 409 Conflict

---

## Example Watchlist List Flow

    Client sends GET /api/watchlist-items
          ↓
    Controller calls Service
          ↓
    Service calls Repository
          ↓
    Repository queries DbSet<WatchlistItem>
          ↓
    Repository returns active Entity list
          ↓
    Service maps Entities to Response DTOs
          ↓
    Controller returns 200 OK

---

## Example Watchlist Delete Flow

    Client sends DELETE /api/watchlist-items/{symbol}
          ↓
    Controller calls Service
          ↓
    Service normalizes Symbol
          ↓
    Service asks Repository for existing record by NormalizedSymbol
          ↓
    If item does not exist or is inactive:
          Service returns false
          Controller returns 404 Not Found
          ↓
    If item exists and is active:
          Service sets IsActive = false
          Service sets UpdatedAtUtc = DateTime.UtcNow
          Repository saves changes
          Controller returns 204 No Content

---

## Allowed Dependency Direction

Dependencies should flow inward toward lower-level implementation details only through clear boundaries.

Recommended dependency direction:

    Controller
        depends on Service

    Service
        depends on Repository abstraction

    Repository
        depends on AppDbContext

    AppDbContext
        depends on Entity models

DTOs are used by Controller and Service for request/response shaping.

Entities are used by Repository and Service for persistence operations.

---

## Layer Dependency Table

| Layer | May Depend On | Should Avoid Depending On |
|---|---|---|
| Controller | Service, DTO | AppDbContext, EF Core queries, database details |
| Service | Repository, Entity, DTO | HTTP-specific behavior, Swagger metadata |
| Repository | AppDbContext, Entity, EF Core | Controller, HTTP status codes, API response DTOs |
| Entity | Basic .NET types, relationships | Controller, Service, Repository, DTO |
| DTO | Basic .NET types | Entity navigation behavior, EF Core configuration |

---

## Watchlist Layer Mapping

| Concept | Layer | Example |
|---|---|---|
| API endpoint | Controller | `GET /api/watchlist-items` |
| Use case | Service | Create, reactivate, delete watchlist item |
| Data access | Repository | Query `WatchlistItems` |
| Database model | Entity | `WatchlistItem` |
| Request model | DTO | `CreateWatchlistItemRequest` |
| Response model | DTO | `WatchlistItemResponse` |
| Database context | Data Access | `AppDbContext` |
| Table representation | EF Core | `DbSet<WatchlistItem>` |

---

## What This Standard Protects

This standard protects the project from:

- Fat controllers
- Business logic in repositories
- Entity models leaking to API responses
- DTOs being used as database models
- Unclear responsibility boundaries
- Difficult-to-test code
- Hard-to-maintain CRUD implementation
- Inconsistent soft delete behavior
- Inconsistent reactivation behavior
- Future refactoring cost

---

## Review Checklist

Before adding a new feature, check:

- Does the Controller only handle HTTP flow?
- Is business logic placed in the Service layer?
- Is database access placed in the Repository layer?
- Are Entities used as persistence models?
- Are DTOs used as request/response models?
- Are Entities avoided as direct API responses?
- Is mapping between Entity and DTO explicit?
- Is symbol normalization handled by the Service?
- Is active duplicate behavior handled by the Service?
- Is inactive reactivation behavior handled by the Service?
- Is soft delete behavior handled by the Service?
- Is the Repository only responsible for data access?
- Is the code easy to explain layer by layer?

---

## Current Status

This standard is used to guide the implementation of:

- Repository Pattern
- LINQ queries
- Watchlist CRUD endpoints
- DTO mapping
- Soft delete behavior
- Inactive record reactivation behavior
- Future price refresh flow
- Future alert evaluation flow
- Future action item workflow

---

## Summary

The project uses a learning-focused layered monolith architecture.

Layer rules:

- Controller handles HTTP request and response flow.
- Service handles use cases and business rules.
- Repository handles database access.
- Entity represents database persistence model.
- DTO represents API request and response contract.

Core rules:

    Do not return Entity models directly from API endpoints.
    Do not put business logic in Controllers.
    Do not put business decisions in Repositories.
    Keep create, duplicate, soft delete, and reactivation decisions in the Service layer.

Use DTOs for API contracts and Entities for database persistence.