# Architecture

This solution follows a layered structure to separate concerns and keep the web layer thin. Projects:

- `Core/` – Domain models and interfaces
- `Infrastructure/` – Implementations for domain interfaces (e.g., storage)
- `Web/` – ASP.NET Core MVC application (controllers, views, static assets)
- `Tests/` – Unit tests

## Solution Layout

- `Core/Models/User.cs` – Domain entity for users
- `Core/Interfaces/Services/IUserService.cs` – Abstraction for user-related operations
- `Infrastructure/Services/UserService.cs` – File-based implementation (`FileUserStore`) persisting users as JSON
- `Web/Controllers/*` – MVC controllers (e.g., `AuthController`, `HomeController`)
- `Web/Views/*` – Razor views and layouts
- `Web/wwwroot/*` – Static assets (CSS/JS/images)

## Dependency Injection (DI)

Configured in `Web/Program.cs`:

- `IUserService` → `FileUserStore`
- `IPasswordHasher<User>` → `PasswordHasher<User>`
- Cookie authentication (see below)

The web layer depends on abstractions from Core and concrete implementations from Infrastructure via DI.

## Authentication

- Cookie-based authentication (`CookieAuthenticationDefaults.AuthenticationScheme`)
- Login and registration handled by `AuthController`
- Password hashing via `Microsoft.AspNetCore.Identity.PasswordHasher<TUser>`

Key endpoints:

- `GET /Auth/Login`, `POST /Auth/Login`
- `GET /Auth/Register`, `POST /Auth/Register`
- `POST /Auth/Logout`

## Routing

Defined in `Web/Program.cs`:

- Default route: `{controller=Home}/{action=Index}/{id?}`
- Contact short route: `contact` → `Home.Contact`

Navbar links use tag helpers to generate URLs consistently.

## Middleware Pipeline

In `Web/Program.cs`:

- `UseHttpsRedirection`, `UseStaticFiles`
- `UseRouting`
- `UseAuthentication`, `UseAuthorization`
- `MapControllerRoute` for routes

In development, Razor runtime compilation is enabled for faster iteration.

## Error Handling

- Developer exception page in development
- `UseExceptionHandler("/Error")` and HSTS in production
- `ErrorController` can be extended to render friendly error pages


