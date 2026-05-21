# GitHub Copilot Instructions

This repository contains the MarketInsight Operations Tracker API project.

## Project Context

MarketInsight Operations Tracker API is a learning-focused ASP.NET Core Web API project designed for backend development practice .

The MVP includes:

- Financial symbol watchlist management
- Public finance API quote retrieval
- SQLite persistence
- EF Core Code First
- Redis quote cache
- RabbitMQ async price refresh
- Background Worker processing
- Swagger/OpenAPI documentation
- XML Summary comments
- Structured logging and basic observability

## Architecture Rules

- Keep the project beginner-friendly and learning-focused.
- Use ASP.NET Core Web API with controllers.
- Keep business logic out of controllers.
- Use services for business logic.
- Use repositories for database access.
- Use DTOs for API request and response models.
- Use entities for database persistence models.
- Use SQLite as the persistent data store.
- Use Redis only for short-term quote caching.
- Use RabbitMQ only for async price refresh messaging.

## Documentation Rules

- Use Markdown files for technical documentation.
- Use XML Summary comments for public controllers, DTOs, service interfaces, and repository interfaces.
- Keep Swagger documentation readable and beginner-friendly.

## Scope Rules

- Do not introduce Azure-specific infrastructure unless explicitly requested.
- Do not add unnecessary enterprise complexity.
- Do not commit API keys, secrets, tokens, or personal learning notes.
- Keep the implementation aligned with the 4-week MVP plan.