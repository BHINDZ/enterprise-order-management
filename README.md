# Enterprise Order Management & API Integration Portal

A full-stack enterprise-style order management application demonstrating **C#, ASP.NET Core/.NET 8, Angular 18, SQL Server, REST API integration, JWT authentication, asynchronous workflows, secure webhooks, automated testing, and API testing with Postman**.

This project was built as a portfolio demonstration for senior full-stack .NET and enterprise API integration roles.

> **Portfolio disclaimer:** This is an independently developed demonstration project. The telecom API is simulated and does not represent proprietary code, data, APIs, or materials from previous employers or clients.

---

## 👨‍💻 About the Project

The application simulates a digital ordering platform where a customer order is submitted through an Angular web application and processed by an ASP.NET Core backend.

The backend integrates with a simulated external telecommunications ordering API. The external system processes the order asynchronously and sends a signed webhook notification back to the application when the order status changes.

The project demonstrates an end-to-end enterprise integration workflow rather than simply implementing CRUD operations.

---

## 🏗️ Architecture

```text
┌──────────────────────────────┐
│        Angular 18 UI         │
│                              │
│ Login / Order Dashboard      │
│ Order Creation               │
│ Status Tracking              │
│ Notifications                │
└──────────────┬───────────────┘
               │
               │ HTTPS / REST / JSON
               │ JWT Bearer Token
               ▼
┌──────────────────────────────┐
│       ASP.NET Core API       │
│            .NET 8            │
│                              │
│ Controllers                  │
│ Application Services         │
│ EF Core                      │
│ Authentication               │
│ Validation / Error Handling  │
└───────┬──────────────┬───────┘
        │              │
        │              │ REST / JSON
        │              ▼
        │      ┌─────────────────────┐
        │      │ Telecom API         │
        │      │ Simulator           │
        │      │                     │
        │      │ Async Order         │
        │      │ Processing          │
        │      └──────────┬──────────┘
        │                 │
        │                 │ HMAC-SHA256
        │                 │ Webhook
        │                 ▼
        │      ┌─────────────────────┐
        │      │ Webhook Endpoint    │
        │      │                     │
        │      │ Signature Validation│
        │      │ Status Update       │
        │      └──────────┬──────────┘
        │                 │
        ▼                 ▼
┌──────────────────────────────────┐
│            SQL Server            │
│                                  │
│ Order persistence                │
│ External Order ID                │
│ Order status                     │
└──────────────────────────────────┘
```

---

## 🔄 End-to-End Order Flow

1. User logs into the Angular application.
2. Angular authenticates against the ASP.NET Core API.
3. API returns a JWT bearer token.
4. Angular includes the JWT when calling protected order endpoints.
5. User creates an order.
6. ASP.NET Core validates and stores the order.
7. The backend submits the order to the simulated external telecom API.
8. The external system returns an external order ID.
9. The simulated telecom system processes the order asynchronously.
10. The telecom simulator sends a signed webhook notification.
11. The API validates the HMAC-SHA256 webhook signature.
12. The corresponding local order is updated.
13. Angular polling detects the status change.
14. The dashboard displays the updated status and user notification.

Example:

```text
PENDING
   ↓
SUBMITTED
   ↓
PROCESSING
   ↓
COMPLETED
```

---

## 🚀 Implemented Features

### Backend

* C# / ASP.NET Core .NET 8 Web API
* RESTful API design
* Entity Framework Core
* SQL Server
* Dependency Injection
* Service-layer architecture
* Request validation
* Global exception handling
* Structured application logging
* Swagger / OpenAPI
* HTTPS
* JWT Bearer authentication
* Protected API endpoints
* External REST API integration
* Typed `HttpClient`
* Async/await processing
* External order ID mapping
* Order status synchronization
* HMAC-SHA256 webhook validation
* Secure webhook signature comparison
* HTTP Problem Details responses

### Frontend

* Angular 18
* TypeScript
* Standalone Angular components
* Reactive HTTP services
* JWT authentication
* HTTP authentication interceptor
* Protected order workflow
* Responsive order dashboard
* Order status counters
* Automatic status polling
* Real-time-style status notifications
* Success/error/info notifications
* Loading and duplicate-submission protection

### API Integration

* REST / JSON communication
* External API abstraction
* Request/response mapping
* External order identifiers
* Asynchronous processing simulation
* Webhook-based status updates
* HMAC-SHA256 webhook security
* Integration error handling
* Integration logging

### Testing

* xUnit
* Moq
* Entity Framework Core InMemory provider
* Order service tests
* Validation tests
* External API failure tests
* Webhook security tests
* Webhook status update tests
* Postman API tests

**Automated backend test result:**

```text
10 total
10 passed
0 failed
0 skipped
```

### Postman validation

The API has also been tested through Postman using the Desktop Agent.

Tested scenarios include:

```text
✓ JWT login
✓ Authenticated GET /api/Orders
✓ Create order
✓ HTTP 201 validation
✓ Generated order ID validation
✓ External order ID validation
✓ Initial SUBMITTED status validation
✓ Unauthorized request → HTTP 401
✓ Invalid webhook signature → HTTP 401
✓ Async webhook → COMPLETED
```

---

## 🔐 Security

### JWT Authentication

Protected order endpoints require a valid JWT bearer token.

Example:

```http
Authorization: Bearer <access-token>
```

Unauthenticated requests are rejected with:

```http
401 Unauthorized
```

### Webhook HMAC Security

Webhook requests are protected using an HMAC-SHA256 signature.

The receiving API:

1. Receives the webhook payload.
2. Reads the webhook signature.
3. Recreates the expected HMAC signature using the shared secret.
4. Compares signatures using a constant-time comparison.
5. Rejects invalid signatures with HTTP 401.
6. Processes the order only after successful verification.

This demonstrates a common pattern for securing server-to-server webhook communication.

### Secret Management

Development secrets are stored using **ASP.NET Core User Secrets** rather than being committed to source control.

The repository does not contain the actual JWT signing key or webhook secret.

---

## 📡 API Endpoints

### Authentication

```http
POST /api/Auth/login
```

### Orders

```http
GET  /api/Orders
POST /api/Orders
```

### Simulated External Telecom API

```http
POST /api/telecom/orders
```

### Webhook

```http
POST /api/webhooks/telecom-order
```

Swagger/OpenAPI is available when running the API locally.

---

## 🧪 Example Order Request

```json
{
  "customerName": "Postman Demo Customer",
  "productCode": "PLAN-100",
  "quantity": 1
}
```

Example response:

```json
{
  "id": 32,
  "customerName": "Postman Demo Customer",
  "productCode": "PLAN-100",
  "quantity": 1,
  "status": 1,
  "externalOrderId": "TEL-20261006045700-3490",
  "createdUtc": "2026-10-06T04:56:58.9776861Z"
}
```

The external system subsequently sends a webhook and the order becomes:

```text
COMPLETED
```

---

## 🛠️ Technology Stack

| Area              | Technology                           |
| ----------------- | ------------------------------------ |
| Backend           | C# / ASP.NET Core / .NET 8           |
| Frontend          | Angular 18 / TypeScript              |
| Database          | SQL Server                           |
| ORM               | Entity Framework Core                |
| API               | REST / JSON                          |
| Authentication    | JWT Bearer                           |
| Webhooks          | HMAC-SHA256                          |
| API Documentation | Swagger / OpenAPI                    |
| API Testing       | Postman                              |
| Automated Testing | xUnit / Moq                          |
| Source Control    | Git / GitHub                         |
| Development       | Visual Studio / VS Code              |
| Local HTTPS       | ASP.NET Core Development Certificate |

---

## 📁 Project Structure

```text
Orkestra_Portfolio_Starter/
│
├── src/
│   └── OrderPortal.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── Migrations/
│       ├── Models/
│       ├── Services/
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   └── OrderPortal.Api.Tests/
│       ├── UnitTest1.cs
│       └── WebhooksControllerTests.cs
│
├── web/
│   └── OrderPortal.Web/
│       ├── src/
│       │   └── app/
│       ├── angular.json
│       ├── package.json
│       └── tsconfig.json
│
├── docs/
│   └── PORTFOLIO.md
│
├── README.md
├── SETUP-GUIDE.txt
└── .gitignore
```

---

## ▶️ Running Locally

### Backend

```powershell
cd src\OrderPortal.Api
dotnet restore
dotnet build
dotnet run
```

The API runs locally using HTTPS.

### Frontend

```powershell
cd web\OrderPortal.Web
npm install
npm.cmd start
```

Angular runs on:

```text
http://localhost:4200
```

### Tests

From the repository root:

```powershell
dotnet test .\tests\OrderPortal.Api.Tests\OrderPortal.Api.Tests.csproj
```

Expected result:

```text
10 passed
0 failed
```

---

## 🗄️ Database

The application uses SQL Server with Entity Framework Core migrations.

The initial database migration is included in:

```text
src/OrderPortal.Api/Migrations/
```

For local development, the project uses SQL Server LocalDB.

---

## 📌 Portfolio Scope

The external telecommunications service in this project is intentionally simulated.

The purpose is to demonstrate transferable enterprise integration capabilities, including:

* REST API integration
* JSON request/response mapping
* authentication
* asynchronous processing
* webhook handling
* secure server-to-server communication
* error handling
* logging
* persistence
* automated testing

It should **not** be interpreted as professional experience integrating with a real telecommunications provider.

---

## 🔮 Potential Future Enhancements

The following features may be added in future iterations:

* Docker / containerization
* GitHub Actions CI/CD
* OAuth 2.0 / OpenID Connect
* Role-based authorization
* Retry and resilience policies using Polly
* Idempotent webhook processing
* Integration audit logging
* Health checks
* Production cloud deployment
* Centralized application monitoring

---

## 🎯 Skills Demonstrated

This project demonstrates practical experience with:

**Backend**

`C#` · `.NET 8` · `ASP.NET Core` · `EF Core` · `SQL Server`

**Frontend**

`Angular 18` · `TypeScript` · `HTML` · `CSS`

**Integration**

`REST` · `JSON` · `HTTP` · `Webhooks` · `HMAC-SHA256` · `Async Workflows`

**Security**

`JWT` · `Bearer Authentication` · `Webhook Signature Validation` · `User Secrets`

**Quality**

`xUnit` · `Moq` · `Postman` · `Swagger/OpenAPI` · `Logging` · `Error Handling`

**Engineering**

`Git` · `GitHub` · `Dependency Injection` · `Service Layer` · `API Design`

---

## 👨‍💻 Author

**Errol Andrew P. Merjudio**

Senior Software Developer | Technical Lead | Full-Stack .NET Developer

20+ years of enterprise software development experience across financial, business, and enterprise application environments.

GitHub:

https://github.com/BHINDZ
