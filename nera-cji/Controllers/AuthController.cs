namespace nera_cji.Controllers;

using nera_cji.Models;
using nera_cji.ViewModels;
using nera_cji.Interfaces.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

public class AuthController : Controller {
    private readonly IUserService _userService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IPasswordHasher<User> passwordHasher,
        ILogger<AuthController> logger) {
        _userService = userService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) {
        if (User?.Identity?.IsAuthenticated == true) {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model) {
        if (!ModelState.IsValid) {
            return View(model);
        }

        var user = await _userService.FindByEmailAsync(model.Email);
        if (user == null) {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (passwordResult == PasswordVerificationResult.Failed) {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInUserAsync(user, model.RememberMe);
        _logger.LogInformation("User {Email} logged in.", user.Email);

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) {
        if (User?.Identity?.IsAuthenticated == true) {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model) {
        if (!ModelState.IsValid) {
            return View(model);
        }

        if (await _userService.EmailExistsAsync(model.Email)) {
            ModelState.AddModelError(nameof(model.Email), "An account with that email already exists.");
            return View(model);
        }

        var user = new User {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim()
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        await _userService.AddAsync(user);
        _logger.LogInformation("User {Email} registered.", user.Email);

        await SignInUserAsync(user, isPersistent: true);

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() {
        if (User?.Identity?.IsAuthenticated == true) {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User {Name} logged out.", User.Identity?.Name);
        }

        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(User user, bool isPersistent) {
        var claims = new List<Claim>
        {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email)
            };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties {
            IsPersistent = isPersistent
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    private IActionResult RedirectToLocal(string? returnUrl) {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
