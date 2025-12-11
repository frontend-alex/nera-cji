namespace nera_cji.Controllers;

using nera_cji.Models;
using nera_cji.ViewModels;
using nera_cji.Interfaces.Services;

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using App.Core;
using Microsoft.EntityFrameworkCore;

public class AuthController : Controller {
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        IAuth0Service auth0Service,
        ILogger<AuthController> logger,
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher) {
        _auth0Service = auth0Service;
        _logger = logger;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
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

        // Check if user exists in database and has a password hash
        // If they do, prioritize database authentication (this ensures password changes work immediately)
        var dbUser = await _dbContext.users
            .AsNoTracking() // Use AsNoTracking to get fresh data from database
            .FirstOrDefaultAsync(u => u.email == model.Email);
        
        bool hasDatabasePassword = dbUser != null && !string.IsNullOrEmpty(dbUser.password_hash);
        
        if (hasDatabasePassword) {
            // User has database password - verify it first (prioritize database over Auth0)
            _logger.LogInformation("User {Email} has database password (length: {HashLength}), checking database authentication FIRST", 
                model.Email, dbUser.password_hash?.Length ?? 0);
            
            // Re-query with tracking for potential updates, but use AsNoTracking first to get fresh data
            var freshDbUser = await _dbContext.users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.email == model.Email);
            
            if (freshDbUser == null) {
                _logger.LogError("User {Email} not found in database during login, but was found earlier!", model.Email);
                ModelState.AddModelError(string.Empty, "User account error. Please contact support.");
                return View(model);
            }
            
            if (string.IsNullOrEmpty(freshDbUser.password_hash)) {
                _logger.LogError("User {Email} has empty password hash in database during login!", model.Email);
                ModelState.AddModelError(string.Empty, "User account error. Please contact support.");
                return View(model);
            }
            
            _logger.LogInformation("Verifying password for user {Email}. Database hash length: {HashLength}", 
                model.Email, freshDbUser.password_hash.Length);
            
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                freshDbUser, 
                freshDbUser.password_hash, 
                model.Password
            );
            
            _logger.LogInformation("Password verification result for user {Email}: {Result}", 
                model.Email, passwordVerificationResult);

            if (passwordVerificationResult == PasswordVerificationResult.Success || 
                passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded) {
                
                // Get tracked entity for sign-in
                var trackedDbUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == model.Email);
                if (trackedDbUser == null) {
                    _logger.LogError("User {Email} not found when getting tracked entity", model.Email);
                    ModelState.AddModelError(string.Empty, "User account error. Please contact support.");
                    return View(model);
                }
                
                // Check if account is active
                if (trackedDbUser.is_active == false) {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact an administrator.");
                    return View(model);
                }

                // Update password hash if rehash is needed
                if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded) {
                    trackedDbUser.password_hash = _passwordHasher.HashPassword(trackedDbUser, model.Password);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Password hash rehashed for user {Email}", trackedDbUser.email);
                }

                await SignInUserAsync(trackedDbUser, model.RememberMe);
                _logger.LogInformation("User {Email} successfully logged in via database authentication.", trackedDbUser.email);

                return RedirectToLocal(model.ReturnUrl);
            } else {
                _logger.LogWarning("Password verification FAILED for database user {Email}. Result: {Result}, Hash length: {HashLength}. NOT trying Auth0 to prevent old password from working.", 
                    model.Email, passwordVerificationResult, freshDbUser.password_hash?.Length ?? 0);
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }
        }
        
        // IMPORTANT: If user has database password, NEVER try Auth0
        // This ensures password changes take effect immediately
        if (!hasDatabasePassword) {
            _logger.LogInformation("User {Email} has NO database password, trying Auth0 authentication", model.Email);
            var auth0Result = await _auth0Service.LoginAsync(model.Email, model.Password);
            
            if (auth0Result.Success) {
                // User authenticated via Auth0
                var user = await EnsureUserInDatabaseAsync(
                    auth0Result.Email ?? model.Email,
                    auth0Result.FullName ?? model.Email
                );

                // Check if account is active
                if (user.is_active == false) {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact an administrator.");
                    return View(model);
                }

                await SignInUserAsync(user, model.RememberMe);
                _logger.LogInformation("User {Email} logged in via Auth0.", user.email);

                return RedirectToLocal(model.ReturnUrl);
            } else {
                _logger.LogWarning("Auth0 login failed for user {Email}", model.Email);
            }
        } else {
            // User has database password - we already checked it above
            // If we reach here, database verification failed
            // DO NOT try Auth0 - this prevents old Auth0 passwords from working
            _logger.LogWarning("User {Email} has database password but verification failed. Auth0 will NOT be tried to prevent old password from working.", model.Email);
        }

        // Authentication failed
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
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

        // Check if user exists in DB
        var existingUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == model.Email);
        if (existingUser != null) {
            ModelState.AddModelError(nameof(model.Email), "An account with that email already exists.");
            return View(model);
        }

        var auth0Result = await _auth0Service.SignupAsync(model.Email, model.Password, model.FullName);
        if (!auth0Result.Success) {
            ModelState.AddModelError(string.Empty, auth0Result.ErrorMessage ?? "Registration failed. Please try again.");
            return View(model);
        }

        var user = await EnsureUserInDatabaseAsync(model.Email, model.FullName);

        await SignInUserAsync(user, isPersistent: true);
        _logger.LogInformation("User {Email} registered via Auth0.", user.email);

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
                new(ClaimTypes.Email, user.email),
                new(ClaimTypes.Role, user.is_admin ? "Admin" : "User")
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

    private async Task<User> EnsureUserInDatabaseAsync(string email, string fullName) {
        try {
            var dbUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == email);
            
            if (dbUser != null) {
                if (dbUser.FullName != fullName) {
                    dbUser.FullName = fullName;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Updated user {Email} in database", email);
                }
                return dbUser;
            }
            
            _logger.LogInformation("Attempting to create user {Email} in database", email);
            
            var newUser = new User {
                email = email,
                FullName = fullName,
                password_hash = string.Empty,
                is_active = true,
                created_at = DateTime.UtcNow,
                is_admin = false
            };

            await _dbContext.users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Successfully created user {Email} in database", email);
            return newUser;

        } catch (Exception ex) {
            _logger.LogError(ex, "Error ensuring user {Email} in database.", email);
            // Fallback: try to fetch again in case of race condition
             var dbUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == email);
             if (dbUser != null) return dbUser;
             throw;
        }
    }
}


