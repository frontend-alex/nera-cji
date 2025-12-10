using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Create()
        {
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
                return RedirectToAction("Index");
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
            return View(events);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _eventService.DeleteAsync(id);

            TempData[deleted ? "Success" : "Error"] =
                deleted ? "Event deleted successfully." :
                "This event cannot be deleted because it has registered participants.";

            return RedirectToAction("AdminDeleteEvents");
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var eventEntity = await _eventService.GetByIdAsync(id);
            if (eventEntity == null)
                return NotFound();

            return View(eventEntity);
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
