namespace nera_cji.Controllers;

using nera_cji.Models;
using nera_cji.ViewModels;
using nera_cji.Interfaces.Services;

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using App.Core;
using Microsoft.EntityFrameworkCore;

public class AuthController : Controller {
    private readonly IUserService _userService;
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public AuthController(
        IUserService userService,
        IAuth0Service auth0Service,
        ILogger<AuthController> logger,
        ApplicationDbContext dbContext) {
        _userService = userService;
        _auth0Service = auth0Service;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) {
        if (User?.Identity?.IsAuthenticated == true) {
            return Redirect("/app/v1/dashboard");
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

        var auth0Result = await _auth0Service.LoginAsync(model.Email, model.Password);
        if (!auth0Result.Success) {
            ModelState.AddModelError(string.Empty, auth0Result.ErrorMessage ?? "Invalid email or password.");
            return View(model);
        }

        var user = await _userService.FindByEmailAsync(model.Email);
        if (user == null) {
            user = new User {
                FullName = auth0Result.FullName ?? model.Email,
                email = auth0Result.Email ?? model.Email,
                password_hash = string.Empty,
                is_active = "1"
            };
            await _userService.AddAsync(user);
            _logger.LogInformation("Created new user in JSON store: {Email}", user.email);
        } else {
            if (!string.IsNullOrEmpty(auth0Result.FullName) && user.FullName != auth0Result.FullName) {
                user.FullName = auth0Result.FullName;
                await _userService.AddAsync(user);
            }
        }

        await EnsureUserInDatabaseAsync(user);

        await SignInUserAsync(user, model.RememberMe);
        _logger.LogInformation("User {Email} logged in via Auth0.", user.email);

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) {
        if (User?.Identity?.IsAuthenticated == true) {
            return Redirect("/app/v1/dashboard");
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

        var auth0Result = await _auth0Service.SignupAsync(model.Email, model.Password, model.FullName);
        if (!auth0Result.Success) {
            ModelState.AddModelError(nameof(model.Email), auth0Result.ErrorMessage ?? "Registration failed. Please try again.");
            return View(model);
        }

        var user = new User {
            FullName = model.FullName.Trim(),
            email = model.Email.Trim(),
            password_hash = string.Empty,
            is_active = "1"
        };

        await _userService.AddAsync(user);
        _logger.LogInformation("User {Email} registered via Auth0.", user.email);

        await EnsureUserInDatabaseAsync(user);

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
                new(ClaimTypes.Email, user.email)
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

        return Redirect("/app/v1/dashboard");
    }

    private async Task EnsureUserInDatabaseAsync(User user) {
        try {
            var dbUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == user.email);
            
            if (dbUser != null) {
                if (dbUser.FullName != user.FullName) {
                    dbUser.FullName = user.FullName;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Updated user {Email} in database", user.email);
                } else {
                    _logger.LogInformation("User {Email} already exists in database", user.email);
                }
                return;
            }
            
            _logger.LogInformation("Attempting to create user {Email} in database", user.email);
            
            bool isActive = user.is_active == "1" || string.IsNullOrEmpty(user.is_active);
            var createdAt = user.created_at == default ? DateTime.UtcNow : user.created_at;
            
            try {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO users (email, password_hash, full_name, is_active, is_admin, created_at) " +
                    "VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                    user.email, 
                    user.password_hash ?? string.Empty,
                    user.FullName ?? user.email,
                    isActive ? 1 : 0,
                    user.is_admin ? 1 : 0,
                    createdAt);
                
                _logger.LogInformation("Successfully created user {Email} in database", user.email);
            } catch (Exception) {
                var existingUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == user.email);
                if (existingUser != null) {
                    _logger.LogInformation("User {Email} already exists in database (insert failed but user found)", user.email);
                } else {
                    throw;
                }
            }
        } catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) {
            _logger.LogError(dbEx, "Database error saving user {Email}. Error: {Message}. Inner: {InnerException}", 
                user.email, dbEx.Message, dbEx.InnerException?.Message);
            if (dbEx.InnerException != null) {
                _logger.LogError("Inner exception details: {Details}", dbEx.InnerException.ToString());
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Could not ensure user {Email} in database. Error: {Message}. Type: {ExceptionType}", 
                user.email, ex.Message, ex.GetType().Name);
            if (ex.InnerException != null) {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
        }
    }
}


