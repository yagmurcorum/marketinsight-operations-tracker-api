# MarketInsight Operations Tracker API - Project Overview

## Project Purpose

MarketInsight Operations Tracker API is a learning-focused .NET backend project designed for preparation.

The goal is to practice backend development concepts such as REST API design, Swagger documentation, SQLite persistence, Redis caching, RabbitMQ-based async processing, structured logging, and basic observability.

## MVP Scope

The MVP will include:

- Financial symbol watchlist management
- Public finance API quote retrieval
- SQLite-based price snapshot persistence
- Redis quote cache
- RabbitMQ-based async price refresh
- Background Worker processing
- Health and dependency status endpoints
- Swagger and XML Summary documentation
- GitHub Issues and GitHub Projects tracking

## Current Phase

Week 1 focuses on:

- Project setup
- GitHub repository and Project Board setup
- ASP.NET Core Web API project creation
- Swagger/OpenAPI verification
- Basic REST API understanding
- Initial technical documentation

## Core Technical Decisions

- Backend framework: ASP.NET Core Web API
- Framework version: .NET 8
- Persistent database: SQLite
- ORM: EF Core Code First
- Cache: Redis for short-term quote cache
- Queue: RabbitMQ for async price refresh
- API testing: Swagger
- Documentation: Markdown and XML Summary
- Tracking: GitHub Issues and GitHub Projects