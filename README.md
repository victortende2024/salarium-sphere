# Salarium Sphere — Multi-Tenant Payroll Platform

A production-grade, full-stack payroll system for multi-tenant SaaS environments.
**ASP.NET Core 8 · React 18 + TypeScript · SQL Server 2022 · EF Core · JWT · Docker**

## ✨ Highlights

- **Multi-tenant architecture** — each tenant gets an isolated database, resolved per-request from a master registry
- **JWT authentication** with role-based access (Admin / Payroll Manager / Employee)
- **Configurable payroll engine** — tiered PAYE tax bands, capped social security, custom benefits, and per-employee deductions stored as data, not code
- **Clean Architecture** — Domain / Application / Infrastructure / API separation
- **Runtime database seeding** — the API migrates and seeds both master and tenant databases on first startup, so reviewers can log in immediately after `docker compose up`
- **React + TanStack Query** frontend with role-scoped dashboards and payslip viewer

## 🚀 Quick Start (Docker — recommended)

The entire stack runs with one command. SQL Server, the API, and the frontend all start automatically, and the database is migrated and seeded on the API's first startup.

### Prerequisites

- Docker Desktop (or Docker Engine + Compose v2)

### Run

```bash
git clone https://github.com/victortende2024/salarium-sphere.git
cd salarium-sphere
docker compose up --build
```

| Service | URL | Notes |
| :--- | :--- | :--- |
| Frontend | http://localhost:3000 | React app (Nginx serving the built bundle) |
| API | http://localhost:8080 | ASP.NET Core Web API |
| Swagger | http://localhost:8080/swagger | API explorer |
| SQL Server | localhost:1433 | SA password set in `docker-compose.yml` |

To stop the stack:

```bash
docker compose down            # keeps database volume
docker compose down -v         # wipes the database (clean slate on next run)
```

### 🔑 Demo Accounts

| Email | Password | Role |
| :--- | :--- | :--- |
| `vickietende@gmail.com` | `Password123` | Admin |
| `john.doe@acme.com` | `Password123` | Employee |

Passwords are BCrypt-hashed (work factor 11). The accounts above are seeded automatically by `DbInitializer` on first API startup.

## 🧱 Solution Structure

```
backend/                          ASP.NET Core 8 solution
├── SalariumSphere.Domain         Entities, enums, domain rules
├── SalariumSphere.Application    Use cases, interfaces, DTOs
├── SalariumSphere.Infrastructure EF Core contexts, migrations, DbInitializer
└── SalariumSphere.API            Controllers, JWT auth, DI wiring, Program.cs

frontend/
└── salarium-sphere-ui            React 18 + Vite + TypeScript

docker-compose.yml                SQL Server + API + frontend orchestration
```

## 🏗️ Key Technical Decisions

- **Tenant resolution via `Tenants.ConnectionString`** — the master DB holds a registry; the `ApplicationDbContext` is constructed per-request from the resolved tenant's connection string, isolating data at the database level.
- **Rules-based payroll calculation** — tax bands, percentage deductions, fixed amounts, and custom benefits are stored as rows (not code), so adding a new country or tax regime requires no redeploy.
- **BCrypt (work factor 11)** for password hashing; **JWT (HS256)** for stateless auth.
- **TanStack Query** for server-state caching on the frontend; React Router for role-gated routes.
- **Idempotent seeding** — `DbInitializer` checks for existing data before inserting, making container restarts safe (no duplicate rows).

## 💻 Local Development (without Docker)

If you prefer to run against a local SQL Server instance:

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- SQL Server 2019 or 2022 (Express or Developer edition)

### Backend

```bash
cd backend/SalariumSphere.API
cp appsettings.Development.example.json appsettings.Development.json
# edit appsettings.Development.json and point MasterConnection at your SQL Server
dotnet run
```

The API runs migrations and seeds on startup (same `DbInitializer` as Docker). No manual migration commands required.

### Frontend

```bash
cd frontend/salarium-sphere-ui
cp .env.example .env
npm install
npm run dev
```

The dev server runs at `http://localhost:5173` and proxies `/api` to `http://localhost:8080`.

## 🧪 Test It

1. Open http://localhost:3000
2. Log in as `vickietende@gmail.com` / `Password123` → admin dashboard
3. Navigate to **Payroll → Run Payroll** for a period covering July–August 2026
4. Log out and sign in as `john.doe@acme.com` / `Password123` → employee view of own payslips

## 📸 Screenshots

_(Add screenshots of the login, admin dashboard, and payslip view here.)_

## 📬 Contact

**Victor Tende** — Full Stack .NET - React/Angular Developer
[GitHub](https://github.com/victortende2024) · vickietende@gmail.com