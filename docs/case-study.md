# Case Study: Building a Multi-Tenant Payroll Platform

**Project:** Salarium Sphere
**Role:** Full-Stack Engineer (sole developer)
**Stack:** ASP.NET Core 8 · React 18 + TypeScript · SQL Server · EF Core · Docker
**Duration:** ~6 weeks (part-time)
**Links:** [GitHub repo](https://github.com/victortende2024/salarium-sphere) · [Live demo setup](https://github.com/victortende2024/salarium-sphere#-quick-start-docker--recommended)

---

## 1. The Problem

Small-to-mid-sized payroll providers in emerging markets (Zimbabwe in this case) typically run payroll for multiple client organizations on a mix of spreadsheets and legacy desktop software. This creates three recurring problems:

1. **Data isolation** — client A's employee records must never be visible to client B, but the tools force shared files
2. **Tax rule churn** — PAYE bands and social security caps change with every budget cycle; hardcoded rules require a redeploy each time
3. **Auditability** — payroll runs must be reproducible and immutable, but spreadsheet-based workflows leave no reliable audit trail

The goal was to build a **multi-tenant SaaS platform** where a single deployment serves many client organizations, each with isolated data, configurable tax rules, and a reliable payroll run history.

---

## 2. Architecture

### Multi-tenancy strategy

I chose a **database-per-tenant** isolation model with a **shared master registry**:

```
Salarium_Master (one per deployment)
  └── Tenants table: { Id, Subdomain, ConnectionString, ... }

Salarium_Acme (one per tenant)
  └── Companies, Departments, Employees, PayrollRuns, ...

Salarium_Beta (one per tenant)
  └── Companies, Departments, Employees, PayrollRuns, ...
```

**Why database-per-tenant over shared-schema-with-TenantId?**
- **Hard isolation** — a query bug in one tenant's code path cannot leak data from another; the connection itself only sees one tenant's data
- **Per-tenant backup/restore** — a client can request a full export without touching others
- **Per-tenant migration timing** — a slow tenant migration doesn't block others
- **Trade-off accepted** — more databases to manage; not a problem at the 10–100 tenant scale this targets

### Request lifecycle

```
1. Client sends request with JWT (contains tenant_id claim)
2. API validates JWT → extracts tenant_id
3. API queries Salarium_Master.Tenants for connection string
4. ApplicationDbContext constructed with that connection string
5. Request executes against the tenant's isolated database
```

The `ApplicationDbContext` is constructed per-request rather than registered as a singleton — the tenant's connection string is only known at request time.

### Solution layers (Clean Architecture)

```
Domain          → Entities, enums, business rules (no dependencies)
Application     → Use cases, interfaces, DTOs
Infrastructure  → EF Core, migrations, DbInitializer, tenant resolution
API             → Controllers, JWT auth, DI wiring, Program.cs
```

Dependencies flow inward: `API → Application → Domain`. `Infrastructure` implements interfaces from `Application`. This means the domain never depends on EF Core or SQL Server, so it can be unit-tested in isolation and re-targeted to another ORM without touching business logic.

---

## 3. The Payroll Rules Engine

The most interesting design decision. Payroll calculation is inherently **data-driven** — tax bands, contribution caps, and per-employee deductions vary by country, by year, and by individual contract.

**Instead of hardcoding rules, I modeled them as first-class entities:**

```csharp
public class PayrollTaxRule : BaseEntity
{
    public string Name { get; set; }                 // "PAYE", "Social Security"
    public DeductionCalculationType CalculationType { get; set; }
    public decimal? PercentageValue { get; set; }    // 5 for 5%
    public decimal? CapAmount { get; set; }          // 200 for capped at $200
    public string? TieredConfigJson { get; set; }    // [{startBand, endBand, rate}, ...]
    public int Priority { get; set; }                // order of application
}
```

**Example: Zimbabwe PAYE (progressive tiers)**

```json
[
  { "startBand": 0,    "endBand": 1000, "rate": 0  },
  { "startBand": 1000, "endBand": 5000, "rate": 20 },
  { "startBand": 5000, "endBand": null, "rate": 30 }
]
```

The engine walks the bands at calculation time:

- For a $3,000 gross: first $1,000 taxed at 0% ($0), next $2,000 at 20% ($400) → **total PAYE = $400**
- For a $7,000 gross: first $1,000 at 0% ($0), next $4,000 at 20% ($800), remaining $2,000 at 30% ($600) → **total PAYE = $1,400**

**Why this matters for the business:** When a country's tax bands change, the fix is a single `UPDATE` on `PayrollTaxRules` — no code deploy, no downtime. A new country launch is a data-insert, not a feature release.

**Custom benefits and per-employee deductions** follow the same pattern. An employee can override their profile's default benefit (e.g., John Doe receives $60 fuel allowance vs. the standard $50) without disrupting other employees on the same profile.

---

## 4. Challenges and How I Solved Them

### Challenge: Idempotent database seeding

**Problem:** Early on, seeding was a manual SQL script run through SSMS. Recruiters cloning the repo would need to:
1. Install SQL Server
2. Create two empty databases
3. Run EF migrations for both contexts
4. Run a long SQL script
5. Remember to substitute their own server name

That's a 20-minute setup with several failure points. During my own testing I ran the seed script twice and ended up with duplicate `PayrollProfile` rows and orphaned benefits — a data integrity problem that took 10 minutes to untangle.

**Solution:** Replaced the SQL script with a `DbInitializer` class that runs on API startup:

```csharp
if (await ctx.Companies.AnyAsync())
{
    logger.LogInformation("Tenant DB already seeded — skipping.");
    return;
}
// ... insert demo data ...
```

**Outcome:**
- Recruiters run **one command** (`docker compose up --build`) instead of a six-step manual sequence
- Container restarts don't duplicate data — verified by `docker compose restart api` and observing "already seeded — skipping" in logs
- The connection string is derived from `MasterConnection` by string substitution, so the same code works locally and inside Docker without environment-specific branches

**Lesson:** Any manual step in the setup path will eventually be skipped or run twice. Idempotency is cheaper than documentation.

### Challenge: Docker build failing on strict TypeScript checks

**Problem:** Local `npm run dev` worked, but Docker's `npm run build` failed with 16 TypeScript errors. The dev server uses esbuild (which silently strips unused imports); the production build runs `tsc -b` (which enforces `noUnusedLocals` and `noUnusedParameters`).

**Solution:** Fixed the real issues rather than disabling the checks:
- The root bug was `companyService.getCompanies()` — a method that didn't exist. Downstream, `useQuery` inferred `{}` for the return type, cascading into "Property 'map' does not exist on type '{}'" errors in `EmployeeForm.tsx`
- Replaced with `companyService.getAll()` and added explicit `= []` defaults on the destructured query results
- Removed 12 unused imports flagged by `tsc`

**Outcome:** `npm run build` passes cleanly, and `docker compose up --build` reproduces exactly what a recruiter will see. I deliberately did **not** disable `noUnusedLocals` — a reviewer running the build should see the same green result.

**Lesson:** Dev servers are forgiving in ways production builds aren't. Running the actual production build (`npm run build`) before committing is now a habit.

### Challenge: Connection strings leaking machine-specific values

**Problem:** The initial `appsettings.json` contained my local machine's SQL Server instance name (`VICKIETENDEPC\MSSQLSERVER01`). Committing this to a public repo:
- Leaks the machine name (minor privacy issue)
- Reads as "broken config" to a recruiter whose server has a different name

**Solution:** Three-layer configuration:
- `appsettings.json` (committed) — generic placeholder (`Server=localhost`)
- `appsettings.Development.json` (**gitignored**) — my real connection string
- `docker-compose.yml` environment variable — overrides everything inside Docker (`Server=sqlserver`, the Docker service name)

.NET's configuration priority handles the override automatically. The same codebase runs locally against my SQL Server, and inside Docker against the container — with no code changes.

**Lesson:** Configuration that differs by environment belongs in environment-specific files, never in the committed base config.

### Challenge: BCrypt hashes vs. plain-text passwords

**Problem:** The initial seed inserted `"Password123"` as the `PasswordHash` column value. The `AuthController` used `BCrypt.Verify(password, hash)` — which threw an exception on plaintext, surfacing as a 500 to the frontend instead of a 401.

**Solution:** A one-off console utility generated BCrypt hashes (work factor 11) and updated the seed users. Then `DbInitializer` was updated to hash at runtime:

```csharp
var hash = BCrypt.Net.BCrypt.HashPassword("Password123", workFactor: 11);
```

**Outcome:** The seed is now self-contained — recruiters cloning the repo get working logins without needing to know about hashing.

**Lesson:** Auth bugs surface as 500s, not 401s, when the failure is in hash parsing rather than comparison. Logging the exception detail (not just "login failed") was what turned this from a 2-hour investigation into a 2-minute diagnosis.

---

## 5. What I'd Do Differently

**Add integration tests earlier.** I wrote zero integration tests during development and only added a manual test flow at the end. If I'd written one test per critical path (login, create employee, run payroll) on day one, the BCrypt and connection-string bugs would have surfaced immediately rather than after hand-testing.

**Deploy to a staging environment from week one.** The pipeline from "works locally" to "works via `docker compose up`" is where most friction lives. If I'd containerized from day one, the eventual Docker setup would have been a natural development artifact instead of a retrofit.

**Model currencies more explicitly.** Currently `Currency` is an enum (`0 = USD, 1 = ZIG, ...`). If the platform ever needs to compute payroll in multiple currencies per employee, a proper `Money` value object with currency as a first-class type would be cleaner than casting between enums and decimals at every boundary.

---

## 6. Outcomes

- **Fully containerized** — 3 services (SQL Server, API, frontend) start with `docker compose up --build` in ~2 minutes on first run, ~20 seconds thereafter
- **Zero-manual-seed** — the API migrates and seeds itself on startup; the initial admin and employee accounts work out of the box
- **Data-driven payroll rules** — adding a country means inserting rows, not writing code
- **Clean Architecture** — Domain has no EF Core dependency; can be unit-tested independently of the persistence layer
- **Repository is portfolio-ready** — README leads with a one-command quick start, includes demo credentials, and documents architectural decisions (this document)

---

## 7. What This Demonstrates

If you're reviewing this for a hiring decision, the specific capabilities I'd highlight:

- **Multi-tenant thinking** — designing for isolation, not just filter-by-TenantId
- **Data-driven design** — recognizing when business logic belongs in code vs. data
- **Debugging systematic failures** — going from "500 error" to "plaintext hash" through layered investigation
- **Container-first delivery** — Docker isn't an afterthought; it's how a reviewer first meets the system
- **Idempotent operations** — every side-effect that could run twice is guarded
- **Clean Architecture discipline** — the Domain project doesn't reference EF Core, HTTP, or SQL Server

If any of these resonate with a role you're hiring for, I'd love to discuss further — my contact details are in the [README](../README.md).