# Setup & Running

## Prerequisites
- .NET 8 SDK

## Build
```bash
# From repo root
dotnet build
```

## Run the Web App
```bash
# From repo root
dotnet run --project Web/Web.csproj
```
- Default route: `/` → `Home/Index`
- Auth: `/Auth/Login`, `/Auth/Register`
- Contact: `/Home/Contact` (also `/contact`)

## Tests
```bash
dotnet test
```

## Environment Notes
- Development enables Razor runtime compilation for views
- App data directory: `App_Data/` under content root (created on demand)


