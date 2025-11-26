using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using nera_cji.Interfaces.Services;
using nera_cji.Models;
using nera_cji.ViewModels;
using App.Core;
using Microsoft.EntityFrameworkCore;

namespace nera_cji.Controllers
{
    public class UserIdResult
    {
        public int Value { get; set; }
    }

    [Authorize]
    [Route("app/v1/events")]
    public class EventsController : Controller
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _dbContext;

        public EventsController(
            ILogger<EventsController> logger,
            IEventService eventService,
            IUserService userService,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _eventService = eventService;
            _userService = userService;
            _dbContext = dbContext;
        }

        [HttpGet]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllAsync();
            return View(events);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new CreateEventViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            var user = await _userService.FindByEmailAsync(userEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            var startTime = model.Date.Date.Add(model.Time);

            var userIntId = await GetUserIdForDatabaseAsync(userEmail);
            
            if (userIntId <= 0)
            {
                _logger.LogError("Could not get valid user ID for {Email}", userEmail);
                ModelState.AddModelError(string.Empty, "User account error. Please try logging out and back in.");
                return View(model);
            }
            
            _logger.LogInformation("Creating event for user {Email} with user ID {UserId}", userEmail, userIntId);

            var eventEntity = new Event
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                Location = model.Location.Trim(),
                StartTime = startTime,
                EndTime = null,
                CreatedBy = userIntId,
                MaxParticipants = model.MaxParticipants,
                Status = model.Status ?? "Draft"
            };

            try
            {
                await _eventService.CreateAsync(eventEntity);
                _logger.LogInformation("Event '{Title}' created by user {Email}", eventEntity.Title, userEmail);
                return RedirectToAction("Index");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error creating event: {Message}", dbEx.Message);
                var errorMessage = "Database error: ";
                if (dbEx.InnerException != null)
                {
                    var innerMsg = dbEx.InnerException.Message;
                    errorMessage += innerMsg;
                    
                    if (innerMsg.Contains("Invalid object name") || innerMsg.Contains("does not exist"))
                    {
                        errorMessage = "The events table does not exist in the database. Please create it first.";
                    }
                    else if (innerMsg.Contains("FOREIGN KEY") || innerMsg.Contains("foreign key constraint"))
                    {
                        errorMessage = "Foreign key constraint error. The created_by user ID may not exist in the users table.";
                    }
                    else if (innerMsg.Contains("Cannot insert the value NULL"))
                    {
                        errorMessage = "A required field is missing. Please check all required fields are filled.";
                    }
                }
                else
                {
                    errorMessage += dbEx.Message;
                }
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL error creating event: {Message}", sqlEx.Message);
                var errorMessage = $"SQL Error: {sqlEx.Message}";
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _eventService.DeleteAsync(id);
            if (deleted)
            {
                _logger.LogInformation("Event {Id} deleted", id);
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var eventEntity = await _eventService.GetByIdAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            return View(eventEntity);
        }

        private async Task<int> GetUserIdForDatabaseAsync(string userEmail)
        {
            try
            {
                var userIdResult = await _dbContext.Database
                    .SqlQueryRaw<UserIdResult>("SELECT id AS Value FROM users WHERE email = {0}", userEmail)
                    .FirstOrDefaultAsync();
                
                if (userIdResult != null && userIdResult.Value > 0)
                {
                    _logger.LogInformation("Found existing user {Email} in database with ID {UserId}", userEmail, userIdResult.Value);
                    return userIdResult.Value;
                }

                _logger.LogInformation("User {Email} not found in database, creating...", userEmail);
                
                var user = await _userService.FindByEmailAsync(userEmail);
                
                string fullName = user?.FullName ?? userEmail;
                string passwordHash = user?.password_hash ?? string.Empty;
                bool isActive = user?.is_active == "1" || string.IsNullOrEmpty(user?.is_active);
                bool isAdmin = user?.is_admin ?? false;
                DateTime createdAt = user?.created_at ?? DateTime.UtcNow;
                
                try
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "INSERT INTO users (email, password_hash, full_name, is_active, is_admin, created_at) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                        userEmail,
                        passwordHash,
                        fullName,
                        isActive ? 1 : 0,
                        isAdmin ? 1 : 0,
                        createdAt);
                    
                    _logger.LogInformation("Created user {Email} in database", userEmail);
                    
                    var newUserIdResult = await _dbContext.Database
                        .SqlQueryRaw<UserIdResult>("SELECT id AS Value FROM users WHERE email = {0}", userEmail)
                        .FirstOrDefaultAsync();
                    
                    if (newUserIdResult != null && newUserIdResult.Value > 0)
                    {
                        _logger.LogInformation("Retrieved user {Email} from database with ID {UserId}", userEmail, newUserIdResult.Value);
                        return newUserIdResult.Value;
                    }
                    
                    _logger.LogWarning("Could not retrieve user ID for {Email} after insert", userEmail);
                    return 1;
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error creating user {Email}. Error: {Message}. Inner: {InnerException}", 
                        userEmail, dbEx.Message, dbEx.InnerException?.Message);
                    if (dbEx.InnerException != null) {
                        _logger.LogError("Database inner exception details: {Details}", dbEx.InnerException.ToString());
                    }
                    
                    var foundUserIdResult = await _dbContext.Database
                        .SqlQueryRaw<UserIdResult>("SELECT id AS Value FROM users WHERE email = {0}", userEmail)
                        .FirstOrDefaultAsync();
                    if (foundUserIdResult != null && foundUserIdResult.Value > 0)
                    {
                        return foundUserIdResult.Value;
                    }
                    
                    return 0;
                }
                catch (Exception insertEx)
                {
                    _logger.LogError(insertEx, "Failed to create user {Email} in database. Error: {Message}. Type: {Type}", 
                        userEmail, insertEx.Message, insertEx.GetType().Name);
                    
                    try
                    {
                        var existingUserIdResult = await _dbContext.Database
                            .SqlQueryRaw<UserIdResult>("SELECT id AS Value FROM users WHERE email = {0}", userEmail)
                            .FirstOrDefaultAsync();
                        if (existingUserIdResult != null && existingUserIdResult.Value > 0)
                        {
                            _logger.LogInformation("Found user {Email} after failed insert with ID {UserId}", userEmail, existingUserIdResult.Value);
                            return existingUserIdResult.Value;
                        }
                    }
                    catch (Exception findEx)
                    {
                        _logger.LogError(findEx, "Error finding user {Email} after failed insert: {Message}", userEmail, findEx.Message);
                    }
                    
                    _logger.LogError("Could not create or find user {Email} in database. Original error: {Error}", 
                        userEmail, insertEx.Message);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting/creating user in database for {Email}: {Message}", userEmail, ex.Message);
                return 0;
            }
        }

    }
}

