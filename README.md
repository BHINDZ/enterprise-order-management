# Enterprise Order Management & API Integration Portal

Portfolio project for demonstrating full-stack .NET and enterprise API integration skills.

> **Important:** This is a portfolio/demo project created independently. It does not contain proprietary code, data, APIs, or materials from previous employers or clients.

## Technologies

- C# / ASP.NET Core Web API
- Entity Framework Core / SQL Server
- Angular / TypeScript
- REST / JSON / HTTP
- Swagger / OpenAPI
- JWT authentication (planned)
- Webhooks and asynchronous workflows (planned)
- xUnit unit testing
- Git / CI/CD

## Scenario

A sales and service team needs a web application for managing customers and orders. The application exposes REST APIs, stores order information in SQL Server, and integrates with a simulated external telecommunications ordering API.

### Flow

Customer -> Angular UI -> ASP.NET Core API -> SQL Server
                                      |
                                      +-> External Telecom API Simulator
                                      |
                                      +<- Order Status Webhook

## Current scope

- Customer management
- Product/plan management
- Order creation
- Order status tracking
- REST API endpoints
- External API integration abstraction
- Validation and error handling

## Planned portfolio enhancements

- JWT authentication and role-based authorization
- Swagger/OpenAPI documentation
- Postman collection
- Webhook endpoint
- Background/asynchronous order processing
- Structured logging
- xUnit tests
- Docker
- GitHub Actions CI/CD
- Angular responsive dashboard
