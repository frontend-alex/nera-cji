# Entities and View Models

## Domain Entities (Core)

### `User` (`Core/Models/User.cs`)
- `Id: Guid` – Unique identifier
- `FullName: string`
- `Email: string`
- `NormalizedEmail: string` – Derived upper-case email (excluded from JSON)
- `PasswordHash: string` – Identity-compatible password hash
- `CreatedAtUtc: DateTime`

## Service Interfaces (Core)

### `IUserService` (`Core/Interfaces/Services/IUserService.cs`)
- `Task<User?> FindByEmailAsync(string email)`
- `Task<bool> EmailExistsAsync(string email)`
- `Task AddAsync(User user)`
- `Task<IReadOnlyCollection<User>> GetAllAsync()`

## Infrastructure Implementations

### `FileUserStore` (`Infrastructure/Services/UserService.cs`)
- Implements `IUserService` using a JSON file under `App_Data/users.json`
- Safe concurrent access via `SemaphoreSlim`
- Uses `System.Text.Json` for serialization

## Web View Models (Web/ViewModels)

### `LoginViewModel`
- `Email`, `Password`, `RememberMe`, `ReturnUrl`

### `RegisterViewModel`
- `FullName`, `Email`, `Password`, `ConfirmPassword`, `ReturnUrl`

### `ContactFormViewModel` and `ContactTopic`
- `Name`, `Email`, `Phone`, `Organization`, `Topic`, `Message`, `ConsentToContact`
- `Topic` uses `ContactTopic` enum to categorize messages


