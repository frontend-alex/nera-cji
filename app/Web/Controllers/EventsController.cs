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
        private readonly ApplicationDbContext _dbContext;

        public EventsController(
            ILogger<EventsController> logger,
            IEventService eventService,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _eventService = eventService;
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

            // Check if user exists in DB
            var user = await _dbContext.users.FirstOrDefaultAsync(u => u.email == userEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found in database.");
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
                Start_Time = startTime,
                End_Time = null,
                Created_By = userIntId,
                Max_Participants = model.MaxParticipants,
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
                    return userIdResult.Value;
                }

                _logger.LogWarning("User {Email} not found in database via direct query", userEmail);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID for {Email}: {Message}", userEmail, ex.Message);
                return 0;
            }
        }

    }
}

