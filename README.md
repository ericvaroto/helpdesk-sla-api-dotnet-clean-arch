# Helpdesk SLA API

A multi-tenant Helpdesk Ticketing API built with **.NET 10**, focused on **Clean Architecture**, **SOLID principles**, **JWT authentication**, **RBAC**, and **SLA management**.

The project is designed to resemble a real-world SaaS backend, emphasizing security, scalability, and maintainability.

---

## Key Features

- Multi-tenant architecture with strict data isolation
- JWT authentication with refresh tokens
- Role-Based Access Control (RBAC)
- SLA tracking based on ticket priority
- Clean Architecture (Domain, Application, Infrastructure, API)
- ASP.NET Identity with GUID/UUID
- PostgreSQL with Entity Framework Core
- Production-oriented structure and conventions

---

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core
- PostgreSQL
- ASP.NET Identity (GUID)
- JWT (HMAC-SHA256)
- Swagger / OpenAPI

---

## Architecture Overview

**Project structure:**

```text
src/
├── Helpdesk.Api
│   └── HTTP layer (controllers, auth, middleware)
├── Helpdesk.Application
│   └── Use cases, DTOs, interfaces, business flows
├── Helpdesk.Domain
│   └── Core business entities and rules
└── Helpdesk.Infrastructure
    └── Persistence, Identity, EF Core, security
```

This separation enforces:

- Clear responsibility boundaries
- Testability
- Framework independence in the Domain layer

---

## Authentication & Authorization

- JWT access tokens (short-lived)
- Refresh tokens persisted in the database
- Claims included in tokens:
  - `sub` – User identifier
  - `role` – User role
  - `tenant_id` – Tenant isolation
- Authorization enforced via RBAC policies

---

## Multi-Tenancy Strategy

- Each request carries a `tenant_id` claim in the JWT
- All multi-tenant entities include a `TenantId` field
- EF Core global query filters ensure tenant data isolation
- Tenant context is resolved per request

---

## SLA Model

SLA targets are calculated based on ticket priority:

| Priority | Target Time |
| -------- | ----------: |
| Low      |    72 hours |
| Medium   |    24 hours |
| High     |     8 hours |
| Critical |     4 hours |

Each ticket exposes:

- `DueAt`
- `IsBreached`
- SLA status (OnTrack, AtRisk, Breached, Met)

---

## Running Locally (without Docker)

### Prerequisites

- .NET 10 SDK
- PostgreSQL running locally
- A database created (example: `helpdesk_db`)

---

### Configuration

Secrets are stored using **.NET User Secrets**.

```bash
cd src/Helpdesk.Api

dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=helpdesk_db;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "Jwt:Key" "YOUR_STRONG_JWT_KEY"
dotnet user-secrets set "Jwt:Issuer" "helpdesk-api"
dotnet user-secrets set "Jwt:Audience" "helpdesk-api"
```

---

### Database

Apply migrations:

```bash
dotnet ef database update   -p src/Helpdesk.Infrastructure   -s src/Helpdesk.Api
```

---

### Run the API

```bash
dotnet run --project src/Helpdesk.Api
```

Swagger UI will be available at:

```text
https://localhost:7006/swagger
```

---

## Project Status

This project is under active development.

Planned next steps:

- Authentication endpoints (login, refresh, me)
- Ticket lifecycle and SLA enforcement
- Auditing and event history
- Integration tests
- Cloud deployment (AWS)

---

## Purpose

This repository exists to demonstrate:

- Production-level backend architecture
- Security best practices
- Clean code and SOLID principles
- Readiness for remote backend roles

---

## License

This project is intended for educational and portfolio purposes.
