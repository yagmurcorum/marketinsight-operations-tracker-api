# XML Summary and Swagger Documentation Standard

## Purpose

This document defines the XML Summary and Swagger documentation standard for the MarketInsight Operations Tracker API.

The goal is to keep API documentation clear, consistent, and visible through Swagger UI.

---

## Scope

This standard applies to public API-facing parts of the project:

- Controllers
- Controller actions
- Request DTOs
- Response DTOs
- Service interfaces
- Repository interfaces

XML Summary comments should explain the responsibility of public contracts. They should not describe every internal implementation detail.

---

## XML Documentation Configuration

XML documentation output must be enabled in the API project file.

Project file:

    src/MarketInsight.Api/MarketInsight.Api.csproj

Required configuration:

    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>

Configuration purpose:

| Setting | Purpose |
|---|---|
| `GenerateDocumentationFile` | Generates the XML documentation file during build |
| `NoWarn 1591` | Suppresses missing XML comment warnings for public members |

---

## Swagger Configuration

Swagger must be configured to read the generated XML documentation file.

Configuration file:

    src/MarketInsight.Api/Program.cs

Required configuration:

    using System.Reflection;

    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

        options.IncludeXmlComments(xmlFilePath);
    });

This allows Swagger UI to display XML Summary descriptions for documented endpoints and models.

---

## Documentation Rules

XML Summary comments must be:

- Written in English
- Short and clear
- Focused on responsibility
- Useful for Swagger readers
- Useful for code review
- Free from unnecessary implementation details

---

## Controller Standard

Controllers should include a short summary that explains the controller responsibility.

Example:

    /// <summary>
    /// Provides basic health information about the API.
    /// </summary>

---

## Action Method Standard

Controller actions should include a summary and return description.

Example:

    /// <summary>
    /// Checks whether the API is running.
    /// </summary>
    /// <returns>Basic health information about the API.</returns>

Actions should also define expected response status codes when possible.

Example:

    [ProducesResponseType(StatusCodes.Status200OK)]

---

## DTO Standard

DTOs should be documented when they are used as request or response models.

Example:

    /// <summary>
    /// Represents basic API health information.
    /// </summary>

DTO properties should be documented when the meaning is not obvious.

Example:

    /// <summary>
    /// UTC timestamp of the response.
    /// </summary>

---

## Service Interface Standard

Service interfaces should document business-level responsibilities.

Example:

    /// <summary>
    /// Provides quote retrieval operations for financial symbols.
    /// </summary>

Service method summaries should describe the business operation, not the implementation.

---

## Repository Interface Standard

Repository interfaces should document data access contracts.

Example:

    /// <summary>
    /// Provides data access operations for watchlist items.
    /// </summary>

Repository method summaries should describe the expected data operation.

---

## Good Example

    /// <summary>
    /// Adds a financial symbol to the watchlist.
    /// </summary>
    /// <param name="request">Watchlist item creation request.</param>
    /// <returns>The created watchlist item.</returns>

---

## Bad Example

    /// <summary>
    /// AddWatchlistItem method.
    /// </summary>

Reason:

- Repeats the method name
- Does not explain the endpoint purpose
- Does not help Swagger users

---

## First Implementation Target

The first endpoint using this standard is:

    GET /api/health

Controller file:

    src/MarketInsight.Api/Controllers/HealthController.cs

Expected endpoint documentation:

    /// <summary>
    /// Checks whether the API is running.
    /// </summary>
    /// <returns>Basic health information about the API.</returns>

This is the first implementation target only. The same standard will be used for future controllers, DTOs, services, and repositories.

---

## Swagger Verification

After implementation, Swagger UI must be checked.

Verification checklist:

- API runs successfully
- Swagger UI opens
- `GET /api/health` is visible
- XML Summary text appears in Swagger
- Endpoint returns `200 OK`
- Response body is valid

---

## Related Files

| File | Purpose |
|---|---|
| `src/MarketInsight.Api/MarketInsight.Api.csproj` | Enables XML documentation output |
| `src/MarketInsight.Api/Program.cs` | Configures Swagger XML comments |
| `src/MarketInsight.Api/Controllers/HealthController.cs` | First XML Summary implementation |
| `docs/03-xml-summary-swagger-standard.md` | Documents the XML Summary standard |

---

## Status

The XML Summary and Swagger documentation standard is defined.

The first implementation target is `GET /api/health`.

This standard will be reused across future API endpoints and public project contracts.