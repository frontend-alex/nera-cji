# Backend: Services, Storage, Error Handling

## Services

### User Service
- Interface: `Core/Interfaces/Services/IUserService.cs`
- Implementation: `Infrastructure/Services/UserService.cs` (class `FileUserStore`)
- Responsibilities:
  - Create and read users
  - Deduplicate by `NormalizedEmail`
  - Provide basic listings for tests/admin features

## Storage (Demo)

For the demo, users are persisted to a JSON file on disk:

- Path: `App_Data/users.json` (relative to content root)
- Serializer: `System.Text.Json`
- Concurrency: `SemaphoreSlim` protects read/write

This keeps the demo self-contained without requiring a database.

## Authentication and Security

- Cookie authentication is configured in `Web/Program.cs`
- Password hashing via `Microsoft.AspNetCore.Identity.PasswordHasher<User>`
- Anti-forgery tokens applied on form POSTs (`[ValidateAntiForgeryToken]`)

## Error Handling & Logging

- Development: developer exception page
- Production: `UseExceptionHandler("/Error")` + HSTS
- Controllers use `ILogger<T>` to log key events (e.g., login/register, contact submissions)
- Validation: Data annotations on view models enforce required fields, lengths, email formats

## Future Work (See Roadmap)

- Replace file storage with MSSQL via EF Core
- Introduce ASP.NET Core Identity for richer auth features
- Add domain validation layer beyond UI validation
- Add structured logging and centralized monitoring


