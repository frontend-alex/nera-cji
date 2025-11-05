# Views and Routing

## Controllers and Routes

- Default route pattern: `{controller=Home}/{action=Index}/{id?}`
- Short route: `contact` → `Home.Contact`

Key endpoints:

- `Home/Index` – Landing page
- `Home/Contact` – Contact form (also available at `/contact`)
- `Auth/Login` – Login form
- `Auth/Register` – Registration form
- `Auth/Logout` – POST only

## Razor Views

- `Web/Views/Shared/_Layout.cshtml` – Base layout
- `Web/Views/Shared/Layouts/_LayoutAuth.cshtml`, `_LayoutRoot.cshtml` – Alternative layouts
- `Web/Views/_ViewImports.cshtml` – Adds tag helpers and common namespaces
- Views define optional `Styles` section rendered by layouts

Example section usage in a view:

```cshtml
@section Styles {
    <link rel="stylesheet" href="~/css/pages/contact.css" asp-append-version="true" />
}
```

## Navbar Links

`Web/Views/Shared/Components/_Navbar.cshtml` uses tag helpers to point to routes:

- Home → `asp-controller="Home" asp-action="Index"`
- Contact → `asp-controller="Home" asp-action="Contact"`
- Login → `asp-controller="Auth" asp-action="Login"`
- Register → `asp-controller="Auth" asp-action="Register"`


