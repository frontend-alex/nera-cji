using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using nera_cji.Interfaces.Services;
using nera_cji.Models;
using nera_cji.ViewModels;
using App.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IEventRegistrationService _registrationService;
        private readonly ILogger<EventsController> _logger;
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _dbContext;

        public EventsController(
            ILogger<EventsController> logger,
            IEventService eventService,
            ApplicationDbContext dbContext,
            IEventRegistrationService registrationService)
        {
            _logger = logger;
            _eventService = eventService;
            _dbContext = dbContext;
            _registrationService = registrationService;
        }

        [HttpGet]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllAsync();

            int? currentUserId = null;
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(userEmail))
            {
                var id = await GetUserIdForDatabaseAsync(userEmail);
                if (id > 0)
                    currentUserId = id;
            }

            var eventIds = events.Select(e => e.Id).ToList();

            var registrationsQuery = _dbContext.event_participants
                .Where(p => eventIds.Contains(p.Event_Id));

            var counts = await registrationsQuery
                .GroupBy(p => p.Event_Id)
                .Select(g => new
                {
                    EventId = g.Key,
                    Count = g.Count(p => p.Status == null || p.Status == "registered")
                })
                .ToListAsync();

            var countDict = counts.ToDictionary(x => x.EventId, x => x.Count);

            var registeredEventIds = new HashSet<int>();
            if (currentUserId.HasValue)
            {
                var regsForUser = await registrationsQuery
                    .Where(p => p.User_Id == currentUserId.Value &&
                               (p.Status == null || p.Status == "registered"))
                    .Select(p => p.Event_Id)
                    .Distinct()
                    .ToListAsync();

                registeredEventIds = regsForUser.ToHashSet();
            }

            ViewBag.RegistrationCounts = countDict;
            ViewBag.RegisteredEventIds = registeredEventIds;

            return View(events);
        }

        [HttpPost("{id:int}/register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int id)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User account not found.";
                return RedirectToAction(nameof(Index));
            }

            var userId = await GetUserIdForDatabaseAsync(userEmail);
            if (userId <= 0)
            {
                TempData["Error"] = "User not found in database.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _registrationService.RegisterAsync(id, userId);

                TempData[success ? "Success" : "Error"] =
                    success ? "You are registered for this event." :
                    "Could not register for this event.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error registering user {Email} for event {Id}", userEmail, id);

                TempData["Error"] = "Unexpected error while registering.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id:int}/unregister")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unregister(int id)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User account not found.";
                return RedirectToAction(nameof(Index));
            }

            var userId = await GetUserIdForDatabaseAsync(userEmail);
            if (userId <= 0)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _registrationService.UnregisterAsync(id, userId);

                TempData[success ? "Success" : "Error"] =
                    success ? "You have been unregistered." :
                    "You are not registered for this event.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error unregistering user {Email} from event {Id}", userEmail, id);

                TempData["Error"] = "Unexpected error.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return View(new CreateEventViewModel());
            }

            var userId = await GetUserIdForDatabaseAsync(userEmail);
            if (userId <= 0)
            {
                return View(new CreateEventViewModel());
            }

            // Get only events created by the current user
            var userEvents = await _dbContext.events
                .Where(e => e.Created_By == userId)
                .OrderByDescending(e => e.Start_Time)
                .ToListAsync();

            ViewBag.UserEvents = userEvents;
            return View(new CreateEventViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            var user = await _dbContext.users.FirstOrDefaultAsync(u => u.email == userEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found in database.");
                return View(model);
            }

            var startTime = model.Date.Date.Add(model.Time);
            var userIntId = await GetUserIdForDatabaseAsync(userEmail);

            if (userIntId <= 0)
            {
                ModelState.AddModelError("", "User ID invalid.");
                return View(model);
            }

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
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                ModelState.AddModelError("", "Error creating event.");
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admindeleteevents")]
        public async Task<IActionResult> AdminDeleteEvents()
        {
            var events = await _eventService.GetAllAsync();
            var users = await _dbContext.users
                .OrderByDescending(u => u.created_at)
                .ToListAsync();
            
            ViewBag.CreateUserModel = new CreateUserViewModel();
            ViewBag.Users = users;
            return View(events);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var events = await _eventService.GetAllAsync();
                ViewBag.CreateUserModel = model;
                ViewBag.Error = "Please correct the errors below.";
                return View("AdminDeleteEvents", events);
            }

            // Check if user already exists in database
            var existingUser = await _dbContext.users.FirstOrDefaultAsync(u => u.email == model.Email);
            if (existingUser != null)
            {
                var events = await _eventService.GetAllAsync();
                ViewBag.CreateUserModel = model;
                ViewBag.Error = "A user with this email already exists.";
                return View("AdminDeleteEvents", events);
            }

            // Create user in Auth0 first
            var auth0Service = HttpContext.RequestServices.GetRequiredService<IAuth0Service>();
            var auth0Result = await auth0Service.SignupAsync(model.Email, model.Password, model.FullName);

            if (!auth0Result.Success)
            {
                var events = await _eventService.GetAllAsync();
                ViewBag.CreateUserModel = model;
                ViewBag.Error = auth0Result.ErrorMessage ?? "Failed to create user in Auth0. Please try again.";
                return View("AdminDeleteEvents", events);
            }

            // Hash the password for database storage
            var passwordHasher = HttpContext.RequestServices.GetRequiredService<IPasswordHasher<User>>();
            var tempUser = new User { email = model.Email };
            var hashedPassword = passwordHasher.HashPassword(tempUser, model.Password);

            // Create the user in database
            var newUser = new User
            {
                email = model.Email,
                FullName = model.FullName,
                password_hash = hashedPassword,
                is_active = model.IsActive,
                is_admin = model.IsAdmin,
                created_at = DateTime.UtcNow
            };

            try
            {
                await _dbContext.users.AddAsync(newUser);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = $"User '{model.FullName}' created successfully in Auth0 and database as {(model.IsAdmin ? "Admin" : "Regular User")}.";
                _logger.LogInformation("Admin created user {Email} with role {Role} in both Auth0 and database", model.Email, model.IsAdmin ? "Admin" : "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email} in database after Auth0 creation", model.Email);
                var events = await _eventService.GetAllAsync();
                ViewBag.CreateUserModel = model;
                ViewBag.Error = "User was created in Auth0 but failed to save to database. Please contact support.";
                return View("AdminDeleteEvents", events);
            }

            return RedirectToAction("AdminDeleteEvents");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("toggle-user-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _dbContext.users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("AdminDeleteEvents");
            }

            // Toggle active status - handle nullable bool
            var currentStatus = user.is_active ?? true;
            var newStatus = !currentStatus;
            user.is_active = newStatus;
            user.updated_at = DateTime.UtcNow;
            
            // Also update Auth0 if user exists there
            var auth0Service = HttpContext.RequestServices.GetRequiredService<IAuth0Service>();
            try
            {
                // Block in Auth0 if deactivating, unblock if activating
                var auth0Success = await auth0Service.BlockUserAsync(user.email, !newStatus);
                if (!auth0Success)
                {
                    _logger.LogWarning("Failed to update Auth0 status for user {Email}, but database was updated", user.email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Auth0 status for user {Email}, but database was updated", user.email);
                // Continue even if Auth0 update fails - database is the source of truth
            }
            
            await _dbContext.SaveChangesAsync();

            var status = newStatus ? "activated" : "deactivated";
            var auth0Message = newStatus ? "" : " (blocked in Auth0)";
            TempData["Success"] = $"User '{user.FullName}' has been {status}{auth0Message}.";
            _logger.LogInformation("Admin {AdminEmail} {Action} user {UserId} ({UserEmail})", 
                User.FindFirstValue(ClaimTypes.Email), 
                newStatus ? "activated" : "deactivated", 
                user.Id, 
                user.email);

            return RedirectToAction("AdminDeleteEvents");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Force delete - admins can delete events even with registered participants
            var deleted = await _eventService.DeleteForceAsync(id);

            TempData[deleted ? "Success" : "Error"] =
                deleted ? "Event deleted successfully." :
                "Event not found or could not be deleted.";

            return RedirectToAction("AdminDeleteEvents");
        }

        [HttpPost("delete-my-event/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMyEvent(int id)
        {
            // Regular users can only delete their own events
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Create");
            }

            var userId = await GetUserIdForDatabaseAsync(userEmail);
            if (userId <= 0)
            {
                TempData["Error"] = "User not found in database.";
                return RedirectToAction("Create");
            }

            // Check if the event exists and belongs to the current user
            var eventEntity = await _eventService.GetByIdAsync(id);
            if (eventEntity == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Create");
            }

            if (eventEntity.Created_By != userId)
            {
                TempData["Error"] = "You can only delete events you created.";
                return RedirectToAction("Create");
            }

            // Check if event has participants - regular users cannot delete events with participants
            var hasParticipants = await _dbContext.event_participants
                .AnyAsync(p => p.Event_Id == id);

            if (hasParticipants)
            {
                TempData["Error"] = "Cannot delete event with registered participants. Please contact an admin.";
                return RedirectToAction("Create");
            }

            // Safe to delete
            var deleted = await _eventService.DeleteAsync(id);

            TempData[deleted ? "Success" : "Error"] =
                deleted ? "Event deleted successfully." :
                "Event could not be deleted.";

            return RedirectToAction("Create");
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var eventEntity = await _eventService.GetByIdAsync(id);
            if (eventEntity == null)
                return NotFound();

            return View(eventEntity);
        }

        [HttpGet("api/details/{id}")]
        public async Task<IActionResult> GetEventDetails(int id)
        {
            var eventEntity = await _eventService.GetByIdAsync(id);
            if (eventEntity == null)
                return Json(new { success = false, message = "Event not found" });

            // Get registration count
            var registrationCount = await _registrationService.GetRegisteredCountAsync(id);
            var capacity = eventEntity.Max_Participants ?? 0;
            var spotsLeft = capacity > 0 ? capacity - registrationCount : capacity;
            if (spotsLeft < 0) spotsLeft = 0;

            // Check if current user is registered
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var isRegistered = false;
            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = await _dbContext.users.FirstOrDefaultAsync(u => u.email == userEmail);
                if (user != null)
                {
                    isRegistered = await _registrationService.IsRegisteredAsync(id, user.Id);
                }
            }

            return Json(new
            {
                success = true,
                eventData = new
                {
                    id = eventEntity.Id,
                    title = eventEntity.Title,
                    description = eventEntity.Description ?? "No description available",
                    location = eventEntity.Location,
                    startTime = eventEntity.Start_Time.ToString("MMMM dd, yyyy HH:mm"),
                    endTime = eventEntity.End_Time?.ToString("MMMM dd, yyyy HH:mm"),
                    cost = eventEntity.Event_Cost,
                    maxParticipants = eventEntity.Max_Participants,
                    spotsLeft = spotsLeft,
                    isFull = capacity > 0 && spotsLeft == 0,
                    isRegistered = isRegistered,
                    status = eventEntity.Status
                }
            });
        }

        private async Task<int> GetUserIdForDatabaseAsync(string userEmail)
        {
            try
            {
                var userIdResult = await _dbContext.Database
                    .SqlQueryRaw<UserIdResult>(
                        "SELECT id AS Value FROM users WHERE email = {0}", userEmail
                    )
                    .FirstOrDefaultAsync();

                return userIdResult?.Value ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user ID for {Email}", userEmail);
                return 0;
            }
        }
    }
}
