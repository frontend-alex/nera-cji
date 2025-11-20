namespace nera_cji.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using nera_cji.ViewModels;

public class AuthController : Controller {
    private const string ScreenHintPropertyKey = "auth0:screen_hint";
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger) {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) {
        if (User?.Identity?.IsAuthenticated == true) {
            return Redirect(DefaultAppRoute);
        }

        return View(new LoginViewModel {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model) {
        if (User?.Identity?.IsAuthenticated == true) {
            return RedirectToLocal(model.ReturnUrl);
        }

        var authProperties = BuildAuthenticationProperties(model.ReturnUrl);
        _logger.LogInformation("Redirecting user to Auth0 for login.");

        return Challenge(authProperties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) {
        if (User?.Identity?.IsAuthenticated == true) {
            return Redirect(DefaultAppRoute);
        }

        return View(new RegisterViewModel {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model) {
        if (User?.Identity?.IsAuthenticated == true) {
            return RedirectToLocal(model.ReturnUrl);
        }

        var authProperties = BuildAuthenticationProperties(model.ReturnUrl);
        authProperties.Items[ScreenHintPropertyKey] = "signup";
        _logger.LogInformation("Redirecting user to Auth0 for signup.");

        return Challenge(authProperties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout(string? returnUrl = null) {
        if (User?.Identity?.IsAuthenticated != true) {
            return RedirectToAction("Index", "Home");
        }

        var redirectUri = ResolveReturnUrl(returnUrl);
        var authProperties = new AuthenticationProperties {
            RedirectUri = redirectUri
        };

        _logger.LogInformation("Signing out user {Name}.", User.Identity?.Name ?? "Unknown");

        return SignOut(
            authProperties,
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    private AuthenticationProperties BuildAuthenticationProperties(string? returnUrl) {
        return new AuthenticationProperties {
            RedirectUri = ResolveReturnUrl(returnUrl)
        };
    }

    private string ResolveReturnUrl(string? returnUrl) {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) {
            return returnUrl;
        }

        return DefaultAppRoute;
    }

    private IActionResult RedirectToLocal(string? returnUrl) {
        return Redirect(ResolveReturnUrl(returnUrl));
    }

    private const string DefaultAppRoute = "/app/v1/dashboard";
}

