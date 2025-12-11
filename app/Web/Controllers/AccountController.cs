namespace nera_cji.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using App.Core;
using nera_cji.ViewModels;
using nera_cji.Models;
using nera_cji.Interfaces.Services;

[Authorize]
[Route("app/v1/account")]
public class AccountController : Controller {
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IAuth0Service _auth0Service;

    public AccountController(
        ILogger<AccountController> logger,
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IAuth0Service auth0Service) {
        _logger = logger;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _auth0Service = auth0Service;
    }

    [HttpGet]
    [HttpGet("index")]
    public async Task<IActionResult> Index() {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var user = await _dbContext.users.FirstOrDefaultAsync(u => u.email == userEmail);

        if (user == null) {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Index", "Dashboard");
        }

        // Get name from claim (set during sign-in from user.FullName)
        var claimName = User.FindFirstValue(ClaimTypes.Name);
        
        // Priority: Database FullName > Claim Name > Email prefix
        string userName;
        if (user != null && !string.IsNullOrWhiteSpace(user.FullName) && 
            user.FullName.Trim() != user.email && user.FullName.Trim() != userEmail)
        {
            // Use database FullName if it's valid and not the email
            userName = user.FullName.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(claimName) && 
                 claimName.Trim() != userEmail && 
                 !claimName.Trim().Contains("@"))
        {
            // Use claim name if it's not the email and doesn't look like an email
            userName = claimName.Trim();
        }
        else
        {
            // Last resort: use email prefix (part before @) capitalized
            var emailPrefix = userEmail.Split('@')[0];
            userName = char.ToUpper(emailPrefix[0]) + emailPrefix.Substring(1);
        }

        var model = new AccountViewModel {
            FullName = userName,
            Email = user.email,
            IsAdmin = user.is_admin,
            IsActive = user.is_active ?? true,
            CreatedAt = user.created_at,
            UpdatedAt = user.updated_at,
            ChangePassword = new ChangePasswordViewModel()
        };

        return View(model);
    }

    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model) {
        _logger.LogInformation("ChangePassword POST received for user {Email}", User.FindFirstValue(ClaimTypes.Email));
        
        // Log what we received
        _logger.LogInformation("Received model - CurrentPassword length: {CurrentLen}, NewPassword length: {NewLen}, ConfirmPassword length: {ConfirmLen}", 
            model?.CurrentPassword?.Length ?? 0, 
            model?.NewPassword?.Length ?? 0, 
            model?.ConfirmPassword?.Length ?? 0);
        
        if (model == null) {
            _logger.LogError("ChangePasswordViewModel model is NULL!");
            TempData["Error"] = "Form data was not received correctly. Please try again.";
            return RedirectToAction("Index");
        }
        
        if (!ModelState.IsValid) {
            _logger.LogWarning("Model validation failed for password change. Errors: {Errors}", 
                string.Join(", ", ModelState.SelectMany(x => x.Value.Errors).Select(e => e.ErrorMessage)));
            
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _dbContext.users.FirstOrDefaultAsync(u => u.email == userEmail);
            
            if (user == null) {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            var accountModel = new AccountViewModel {
                FullName = user.FullName,
                Email = user.email,
                IsAdmin = user.is_admin,
                IsActive = user.is_active ?? true,
                CreatedAt = user.created_at,
                UpdatedAt = user.updated_at,
                ChangePassword = model
            };

            return View("Index", accountModel);
        }
        
        _logger.LogInformation("Model validation passed, proceeding with password change");

        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var currentUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == currentUserEmail);

        if (currentUser == null) {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Index");
        }

        // Check if new password is the same as current password
        if (model.NewPassword == model.CurrentPassword) {
            ModelState.AddModelError(nameof(model.NewPassword), "New password must be different from your current password.");
            var accountModel = new AccountViewModel {
                FullName = currentUser.FullName,
                Email = currentUser.email,
                IsAdmin = currentUser.is_admin,
                IsActive = currentUser.is_active ?? true,
                CreatedAt = currentUser.created_at,
                UpdatedAt = currentUser.updated_at,
                ChangePassword = model
            };
            return View("Index", accountModel);
        }
        
        // Verify current password
        bool passwordValid = false;
        
        _logger.LogInformation("Verifying current password for user {Email}", currentUserEmail);
        
        // Check if user has Auth0 account (try Auth0 login first)
        var auth0Result = await _auth0Service.LoginAsync(currentUserEmail, model.CurrentPassword);
        if (auth0Result.Success) {
            passwordValid = true;
            _logger.LogInformation("Current password verified via Auth0 for user {Email}", currentUserEmail);
        }
        // If Auth0 fails, check database password
        else if (!string.IsNullOrEmpty(currentUser.password_hash)) {
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                currentUser, 
                currentUser.password_hash, 
                model.CurrentPassword
            );
            passwordValid = passwordVerificationResult == PasswordVerificationResult.Success || 
                           passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded;
            
            _logger.LogInformation("Current password verification result for user {Email}: {Result}", 
                currentUserEmail, passwordVerificationResult);
        }

        if (!passwordValid) {
            _logger.LogWarning("Current password verification FAILED for user {Email}", currentUserEmail);
            ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
            var accountModel = new AccountViewModel {
                FullName = currentUser.FullName,
                Email = currentUser.email,
                IsAdmin = currentUser.is_admin,
                IsActive = currentUser.is_active ?? true,
                CreatedAt = currentUser.created_at,
                UpdatedAt = currentUser.updated_at,
                ChangePassword = model
            };
            return View("Index", accountModel);
        }
        
        _logger.LogInformation("Current password verified successfully for user {Email}", currentUserEmail);

        // Always update password in database first (this is our source of truth)
        var oldPasswordHash = currentUser.password_hash;
        var newPasswordHash = _passwordHasher.HashPassword(currentUser, model.NewPassword);
        
        _logger.LogInformation("Starting password update for user {Email} (ID: {UserId}). Old hash length: {OldLength}, New hash length: {NewLength}", 
            currentUserEmail, currentUser.Id, oldPasswordHash?.Length ?? 0, newPasswordHash?.Length ?? 0);
        
        try {
            // Use ExecuteSqlRaw to directly update the database, bypassing EF tracking issues
            _logger.LogInformation("Executing SQL UPDATE for user {Email} (ID: {UserId}). New hash length: {HashLength}", 
                currentUserEmail, currentUser.Id, newPasswordHash?.Length ?? 0);
            
            // Use parameterized query to prevent SQL injection and ensure proper escaping
            var updateTime = DateTime.UtcNow;
            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE users SET password_hash = {0}, updated_at = {1} WHERE id = {2}",
                newPasswordHash,
                updateTime,
                currentUser.Id
            );
            
            // Force save any pending changes
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("SQL UPDATE completed. Rows affected: {RowsAffected} for user {Email} (ID: {UserId})", 
                rowsAffected, currentUserEmail, currentUser.Id);
            
            if (rowsAffected != 1) {
                _logger.LogError("CRITICAL: SQL update affected {RowsAffected} rows instead of 1 for user {Email} (ID: {UserId})!", 
                    rowsAffected, currentUserEmail, currentUser.Id);
                TempData["Error"] = $"Password update failed. SQL affected {rowsAffected} rows. Please try again or contact support.";
                return RedirectToAction("Index");
            }
            
            // Immediately verify with a raw SQL query using FromSqlRaw
            var verifyUser = await _dbContext.users
                .FromSqlRaw($"SELECT * FROM users WHERE id = {currentUser.Id}")
                .AsNoTracking()
                .FirstOrDefaultAsync();
            
            if (verifyUser == null) {
                _logger.LogError("CRITICAL: User not found after SQL update!");
                TempData["Error"] = "Password update failed - user not found. Please contact support.";
                return RedirectToAction("Index");
            }
            
            _logger.LogInformation("Verification query result: Hash length = {Length} for user {Email}", 
                verifyUser.password_hash?.Length ?? 0, currentUserEmail);
            
            if (verifyUser.password_hash != newPasswordHash) {
                _logger.LogError("CRITICAL: Password hash mismatch after SQL update! Expected length: {Expected}, Got length: {Actual}", 
                    newPasswordHash?.Length ?? 0, verifyUser.password_hash?.Length ?? 0);
                TempData["Error"] = "Password update verification failed. The password was not saved correctly. Please try again.";
                return RedirectToAction("Index");
            }
            
            _logger.LogInformation("SQL UPDATE verified successfully - hash matches in database");
            
            // Clear any tracked entities to force fresh read
            _dbContext.ChangeTracker.Clear();
            
            // Verify by querying the database directly
            var verificationUser = await _dbContext.users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);
            
            if (verificationUser == null) {
                _logger.LogError("CRITICAL: User {Email} (ID: {UserId}) not found after password update!", currentUserEmail, currentUser.Id);
                TempData["Error"] = "CRITICAL: User not found after password update. Please contact support.";
                return RedirectToAction("Index");
            }
            
            _logger.LogInformation("Verification query completed. Retrieved hash length: {HashLength} for user {Email}", 
                verificationUser.password_hash?.Length ?? 0, currentUserEmail);
            
            // Compare hashes
            if (verificationUser.password_hash != newPasswordHash) {
                _logger.LogError("CRITICAL: Password hash mismatch after SQL update! Expected length: {ExpectedLength}, Actual length: {ActualLength} for user {Email}", 
                    newPasswordHash?.Length ?? 0,
                    verificationUser.password_hash?.Length ?? 0,
                    currentUserEmail);
                TempData["Error"] = "Password update verification failed. Please try again or contact support.";
                return RedirectToAction("Index");
            }
            
            _logger.LogInformation("Password hash verified successfully - hash matches in database for user {Email}", currentUserEmail);
            
            // Additional verification: Try to verify the new password works
            var testVerification = _passwordHasher.VerifyHashedPassword(
                verificationUser, 
                verificationUser.password_hash, 
                model.NewPassword
            );
            
            if (testVerification != PasswordVerificationResult.Success && 
                testVerification != PasswordVerificationResult.SuccessRehashNeeded) {
                _logger.LogError("CRITICAL: New password verification FAILED immediately after saving for user {Email}! Verification result: {Result}", 
                    currentUserEmail, testVerification);
                TempData["Error"] = "CRITICAL ERROR: Password was saved but verification failed. Please contact support immediately.";
                return RedirectToAction("Index");
            }
            
            _logger.LogInformation("New password verification test PASSED for user {Email}. Verification result: {Result}", 
                currentUserEmail, testVerification);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to save password to database for user {Email}", currentUserEmail);
            TempData["Error"] = "Failed to update password. Please try again.";
            var accountModel = new AccountViewModel {
                FullName = currentUser.FullName,
                Email = currentUser.email,
                IsAdmin = currentUser.is_admin,
                IsActive = currentUser.is_active ?? true,
                CreatedAt = currentUser.created_at,
                UpdatedAt = currentUser.updated_at,
                ChangePassword = new ChangePasswordViewModel()
            };
            return View("Index", accountModel);
        }
        
        // Database password update is complete and verified
        // Now try to update Auth0 (but don't fail if it doesn't work)
        bool auth0Updated = false;
        try {
            _logger.LogInformation("Attempting Auth0 password update for user {Email}", currentUserEmail);
            var auth0UserId = await _auth0Service.GetUserIdByEmailAsync(currentUserEmail);
            
            if (!string.IsNullOrEmpty(auth0UserId)) {
                auth0Updated = await _auth0Service.UpdatePasswordAsync(currentUserEmail, model.NewPassword);
                _logger.LogInformation("Auth0 password update result for {Email}: {Result}", currentUserEmail, auth0Updated);
            }
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Auth0 update failed for {Email}, but database password was updated", currentUserEmail);
        }

        // Set clear success message - ALWAYS set this
        string successMessage;
        if (auth0Updated) {
            successMessage = "✅ Password changed successfully! Your password has been updated in both the database and Auth0. You can now log in with your new password.";
        } else {
            successMessage = "✅ Password changed successfully! Your password has been updated in the database. You can now log in with your new password.";
        }
        
        TempData["Success"] = successMessage;
        _logger.LogInformation("Setting TempData Success message: {Message}", successMessage);
        _logger.LogInformation("Password change completed successfully for user {Email}. Database: UPDATED, Auth0: {Auth0Status}", 
            currentUserEmail, auth0Updated ? "UPDATED" : "SKIPPED/FAILED");

        // Ensure TempData is kept
        TempData.Keep("Success");
        
        return RedirectToAction("Index");
    }
}

public class AccountViewModel {
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ChangePasswordViewModel ChangePassword { get; set; } = new ChangePasswordViewModel();
}

