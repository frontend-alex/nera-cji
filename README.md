# NERA-CGI Web Application

A demo ASP.NET Core MVC application for event administration with simple authentication, contact form, and basic pages. This repository is structured as a clean, layered solution to keep domain, infrastructure, and web layers separate.

## Quick Start

- Requirements: .NET 8 SDK
- Build all projects:
  - `dotnet build`
- Run the web app:
  - `dotnet run --project app/Web/Web.csproj`
- Default route: `/` → `Home/Index`
- Auth: `/Auth/Login`, `/Auth/Register`
- Contact page: `/Home/Contact` (also available at `/contact`)

## Documentation

Comprehensive documentation lives in the `docs/` folder:

- [Architecture](docs/Architecture.md) – Solution layout, DI, middleware, authentication
- [Entities](docs/Entities.md) – Domain models and view models
- [Views & Routing](docs/ViewsAndRouting.md) – Views, layouts, sections, route mapping
- [Backend](docs/Backend.md) – Services, JSON storage, logging/error handling
- [Setup](docs/Setup.md) – Environment, build, run, test
- [Roadmap](docs/Roadmap.md) – Future MSSQL/EF Core, Identity, CI/CD

## Repository Layout

Application code is under `app/` and tests under `tests/`:

```
app/
  Core/
  Infrastructure/
  Web/
Tests/
  Tests.csproj
Docs/
Scripts/
```

## Scripts

From the repo root:

- PowerShell: `./scripts/start.ps1 build|run|test|clean|watch|publish|restore`
- CMD: `scripts\start.bat build|run|test|clean|watch|publish|restore`

Both scripts target `app/Web` and work with the current layout.


