# Roadmap

## Data Storage
- Migrate from JSON file to MSSQL using Entity Framework Core
- Introduce migrations, seed data, and DB versioning
- Replace `FileUserStore` with `EfCoreUserStore`

## Authentication & Authorization
- Adopt ASP.NET Core Identity for richer user management
- Add email confirmation, password reset, lockout, MFA
- Role-based authorization for admin features

## Architecture & DX
- Add domain validations and result types
- Add integration tests and test data builders
- Introduce Serilog + structured logging
- CI/CD pipeline (build/test/lint) and deployment scripts

## UI & UX
- Expand layout system, add partials/components
- Client-side validation summary improvements
- Accessibility and responsive refinements


