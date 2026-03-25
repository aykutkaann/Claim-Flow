# ClaimFlow - Insurance Claims Management System

A full-stack insurance claims management system built with **.NET 10**, **PostgreSQL 16 + pgvector**, **RabbitMQ**, and **React**. Features a state machine-driven claims workflow, AI-powered fraud detection, advanced SQL reporting, and event-driven architecture.

> **Transparency note:** The backend (.NET, database design, architecture) was written by me as a learning project. The React frontend was generated with AI tooling and is not representative of my frontend skills.

---

## Architecture

```
┌─────────────┐     ┌─────────────────┐     ┌──────────────┐
│  React UI   │────▸│  .NET 10 API    │────▸│ PostgreSQL   │
│  (port 3000)│     │  (port 5000)    │     │ + pgvector   │
└─────────────┘     └────────┬────────┘     └──────────────┘
                             │
                    ┌────────▼────────┐
                    │    RabbitMQ     │
                    │  (MassTransit)  │
                    └─────────────────┘
```

### Clean Architecture Layers

| Layer | Responsibility |
|---|---|
| `ClaimFlow.Domain` | Entities, enums, domain events, state machine. Zero framework dependencies. |
| `ClaimFlow.Application` | CQRS commands/queries (MediatR), DTOs, validation (FluentValidation), interfaces |
| `ClaimFlow.Infrastructure` | EF Core, Dapper queries, MassTransit consumers, Hangfire jobs, outbox processor |
| `ClaimFlow.Api` | Minimal API endpoints, middleware, DI composition root |
| `ClaimFlow.Tests` | Integration tests with Testcontainers (real PostgreSQL) |

---

## Key Features

### 1. Claims Processing - State Machine + Event-Driven

Claims follow a strict lifecycle enforced by the [Stateless](https://github.com/dotnet-state-machine/stateless) library:

```
Submitted → UnderReview → DocumentsRequested → UnderReview
                        → UnderInvestigation → Approved → PaymentScheduled → Paid → Closed
                                             → Rejected → Appeal → UnderReview (re-enters cycle)
```

- Invalid transitions throw exceptions (e.g., can't Approve a Submitted claim directly)
- Every transition is recorded in `ClaimStatusHistory` for full audit trail
- Domain events (`ClaimSubmittedEvent`, `ClaimApprovedEvent`, etc.) are published on each transition

### 2. Transactional Outbox Pattern

Events are saved to an `outbox_messages` table in the **same database transaction** as the claim change. A background worker (`OutboxProcessor`) polls every 5 seconds and publishes to RabbitMQ via MassTransit. This guarantees no events are lost if the app crashes between save and publish.

### 3. AI-Powered Fraud Detection (pgvector)

When a claim is submitted, its description is embedded as a 1536-dimension vector. The fraud scoring system combines 4 factors (0-100 score):

| Factor | Max Points | Logic |
|---|---|---|
| Vector similarity to known fraud | 40 | Cosine distance via `<=>` operator against flagged claims |
| Amount anomaly | 20 | Claim amount vs. historical average for the policy |
| Timing | 20 | Claims filed within 30/90 days of policy start date |
| Frequency | 20 | Number of claims in the last 6 months |

The similarity query uses Dapper with raw SQL:

```sql
SELECT c."Id", c."Description",
    1 - (c."Embedding" <=> @embedding::vector) AS similarity_score
FROM claims c
WHERE c."IsFraud" = true
ORDER BY c."Embedding" <=> @embedding::vector
LIMIT 5;
```

### 4. Advanced PostgreSQL Reporting (Dapper)

All reporting queries use Dapper instead of EF Core to demonstrate knowledge of when each tool is appropriate:

- **Materialized views** for monthly loss ratios (claims paid vs. premiums collected)
- **Window functions** for rolling 3-month average claim amounts per product type
- **CTEs** for hierarchical agent performance reports
- **Full-text search** with `tsvector` and `ts_rank` for searching claim descriptions

### 5. Multi-Tenancy

Every entity carries a `TenantId`. Policies inherit their tenant from the customer (business rule: a policy always belongs to the same branch as its customer). PostgreSQL Row-Level Security can be layered on top.

### 6. Background Jobs (Hangfire)

| Job | Schedule | Description |
|---|---|---|
| `PremiumRenewalReminderJob` | Daily 09:00 | Flags policies expiring within 30 days |
| `StaleClaimAlertJob` | Daily 08:00 | Alerts on claims stuck in review > 7 days |
| `PolicyExpirationJob` | Daily 00:00 | Auto-expires policies past their end date |
| `MonthlyStatementJob` | 1st of month 06:00 | Generates monthly policyholder statements |

### 7. Observability

- **Serilog** structured logging with correlation IDs, console sink
- **OpenTelemetry** tracing across HTTP, ASP.NET Core, and MassTransit
- **Health checks** at `/health/ready` (database) and `/health/live` (app running)

---

## Tech Stack

| Category | Technology |
|---|---|
| Framework | .NET 10, ASP.NET Core Minimal APIs |
| Database | PostgreSQL 16 + pgvector extension |
| ORM | EF Core 10 (Npgsql) + Dapper for reporting |
| Messaging | RabbitMQ + MassTransit 8 |
| CQRS | MediatR with pipeline behaviors |
| Validation | FluentValidation |
| State Machine | Stateless |
| Background Jobs | Hangfire with PostgreSQL storage |
| Logging | Serilog |
| Tracing | OpenTelemetry with OTLP exporter |
| Testing | xUnit + Testcontainers |
| Connection Pooling | PgBouncer |
| Frontend | React 19 (AI-generated) |
| Containerization | Docker + Docker Compose |
| CI/CD | GitHub Actions |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 20+](https://nodejs.org/) (only if running frontend locally)

### Option 1: Docker Compose (everything containerized)

```bash
git clone https://github.com/yourusername/ClaimFlow.git
cd ClaimFlow
docker compose up --build -d
```

| Service | URL |
|---|---|
| API (Swagger) | http://localhost:5000/swagger |
| Frontend | http://localhost:3000 |
| RabbitMQ Dashboard | http://localhost:15672 (claimflow / claimflow_dev_2026) |
| Hangfire Dashboard | http://localhost:5000/hangfire |

### Option 2: Local Development

```bash
# Start infrastructure only
docker compose up postgres rabbitmq pgbouncer -d

# Run the API
cd src/ClaimFlow.Api
dotnet run

# (Optional) Run the frontend
cd client
npm install
npm start
```

API will be at `https://localhost:5001/swagger`, frontend at `http://localhost:3000`.

---

## API Testing Scenario

Run these requests in order via Swagger (`/swagger`) or Postman to test the full claims lifecycle:

### Step 1: Create a Tenant

```http
POST /api/tenants
```
```json
{
  "name": "Istanbul Regional Office",
  "code": "IST-01"
}
```
Save the returned `id`.

### Step 2: Create a Customer

```http
POST /api/customers
```
```json
{
  "fullName": "Ahmet Yilmaz",
  "email": "ahmet.yilmaz@email.com",
  "tenantId": "<tenant-id>"
}
```
Save the returned `id`.

### Step 3: Create a Policy

```http
POST /api/policies
```
```json
{
  "policyNumber": "POL-2026-00001",
  "customerId": "<customer-id>",
  "productType": 1,
  "startDate": "2026-01-01",
  "endDate": "2027-01-01"
}
```
Save the returned `id`. (`productType`: 0=Health, 1=Auto, 2=Home, 3=Travel)

### Step 4: Submit a Claim

```http
POST /api/claims
```
```json
{
  "policyId": "<policy-id>",
  "description": "Rear-ended at traffic light on E5 highway. Bumper and tail lights damaged.",
  "claimedAmount": 15000.00
}
```
Save the returned `id`.

### Step 5: Walk Through the Claim Lifecycle

Each step is a `PATCH /api/claims/<claim-id>/transition`:

**5a. Start Review**
```json
{ "trigger": 0, "changedBy": "system", "notes": "Auto-assigned to review queue" }
```

**5b. Request Documents**
```json
{ "trigger": 1, "changedBy": "adjuster-ali", "notes": "Need police report and damage photos" }
```

**5c. Documents Received (back to review)**
```json
{ "trigger": 0, "changedBy": "system", "notes": "Documents uploaded by policyholder" }
```

**5d. Approve**
```json
{ "trigger": 3, "changedBy": "adjuster-ali", "notes": "Damage confirmed. Approved 12000 TL." }
```

**5e. Schedule Payment**
```json
{ "trigger": 5, "changedBy": "finance-dept", "notes": "Payment scheduled for next week" }
```

**5f. Confirm Payment**
```json
{ "trigger": 6, "changedBy": "finance-dept", "notes": "Wire transfer completed" }
```

**5g. Close**
```json
{ "trigger": 7, "changedBy": "adjuster-ali", "notes": "Claim resolved successfully" }
```

### Step 6: Verify Audit Trail

```http
GET /api/claims/<claim-id>
```

Response includes `statusHistory` array with 7 entries showing the complete lifecycle.

### Step 7: Run Fraud Check

```http
GET /api/claims/<claim-id>/fraud-check
```

Returns a risk score (0-100) with breakdown of risk factors.

### Step 8: Test Invalid Transition

Submit a new claim, then try to approve it directly (trigger 3) without starting review first. The API should return an error because the state machine blocks it.

---

## Project Structure

```
ClaimFlow/
├── src/
│   ├── ClaimFlow.Api/                  # Composition root, endpoints, middleware
│   │   ├── Extensions/                 # Minimal API endpoint groups
│   │   ├── Middlewares/                # Exception handling
│   │   └── Program.cs                 # DI configuration
│   │
│   ├── ClaimFlow.Application/          # Use cases, no framework dependencies
│   │   ├── Behaviors/                  # MediatR pipeline (validation)
│   │   ├── DTOs/                       # Data transfer objects
│   │   ├── Features/                   # CQRS commands & queries
│   │   │   ├── Claims/
│   │   │   ├── Customers/
│   │   │   ├── Policies/
│   │   │   └── Tenants/
│   │   ├── Interfaces/                 # Abstractions (IAppDbContext, IFraudDetectionService)
│   │   └── Messages/                   # MassTransit message contracts
│   │
│   ├── ClaimFlow.Domain/               # Pure domain logic
│   │   ├── Entities/                   # Claim, Policy, Customer, Tenant, etc.
│   │   ├── Enums/                      # ClaimStatus, ClaimTrigger, ProductType, etc.
│   │   ├── Events/                     # Domain events (INotification)
│   │   └── StateMachines/              # ClaimStateMachine (Stateless)
│   │
│   └── ClaimFlow.Infrastructure/       # External concerns
│       ├── BackgroundServices/         # OutboxProcessor
│       ├── Consumers/                  # MassTransit RabbitMQ consumers
│       ├── Data/
│       │   ├── Configurations/         # EF Core Fluent API configurations
│       │   └── Migrations/
│       ├── Jobs/                       # Hangfire recurring jobs
│       └── Services/                   # FraudDetection, Embedding, Reporting
│
├── tests/
│   └── ClaimFlow.Tests/               # Integration tests with Testcontainers
│
├── client/                             # React frontend (AI-generated)
│   ├── src/
│   │   ├── components/
│   │   └── pages/
│   ├── Dockerfile
│   └── nginx.conf
│
├── docker-compose.yml
├── pgbouncer.ini
└── .github/workflows/ci.yml
```

---

## Running Tests

```bash
# Docker must be running (Testcontainers spins up a real PostgreSQL)
dotnet test
```

Tests include:
- Tenant creation and unique constraint validation
- Full claim lifecycle through the state machine
- Invalid transition rejection
- Multi-tenancy data isolation
- Claim persistence with status history

---

## License

This project was built for learning purposes.
