# Salarium Sphere — Multi-Tenant Payroll Platform

Production-grade full-stack payroll system.
**ASP.NET Core 8 · React 18 + TypeScript · SQL Server · EF Core · JWT**

## Structure

- `backend/` — ASP.NET Core solution (Domain, Application, Infrastructure, API)
- `frontend/salarium-sphere-ui/` — React 18 + Vite + TypeScript

## Quick Start

### Backend
```bash
cd backend
cp SalariumSphere.API/appsettings.Development.example.json SalariumSphere.API/appsettings.Development.json
# edit the connection string to point at your SQL Server
dotnet ef database update --context MasterDbContext --project SalariumSphere.Infrastructure
dotnet ef database update --context ApplicationDbContext --project SalariumSphere.Infrastructure
dotnet run --project SalariumSphere.API


Front End 
cd frontend/salarium-sphere-ui
cp .env.example .env
npm install
npm run dev


Demo Accounts
Email	Password	Role
vickietende@gmail.com	Password123	Admin
john.doe@acme.com	Password123	Employee


Contact
Victor Tende — GitHub