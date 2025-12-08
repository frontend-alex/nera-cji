using System;
using System.Linq;
using System.Security.Claims;
using App.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using nera_cji.ViewModels;

namespace nera_cji.Controllers {
    [Authorize]
    [Route("app/v1/dashboard")]
    public class DashboardController : Controller {
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public DashboardController(
            ILogger<DashboardController> logger,
            ApplicationDbContext dbContext) {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        [HttpGet("index")]
        public async Task<IActionResult> Index() {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "User";
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            var model = new DashboardViewModel {
                UserName = userName,
                UserEmail = userEmail
            };

            // Look up user in DB
            var user = await _dbContext.users
                .FirstOrDefaultAsync(u => u.email == userEmail);

            if (user == null) {
                // No DB row for this user yet – just show zeroes
                return View(model);
            }

            // 1) Total events
            model.TotalEvents = await _dbContext.events.CountAsync();

            // 2) Upcoming events (Start_Time in the future)
            var allEvents = await _dbContext.events.ToListAsync();
            model.UpcomingEvents = allEvents.Count(e => e.Start_Time > DateTime.UtcNow);

            // 3) Registrations for this user
            var registrations = await _dbContext.event_participants
                .Where(p => p.User_Id == user.Id &&
                            (p.Status == null || p.Status == "registered"))
                .ToListAsync();

            model.MyRegistrations = registrations.Count;

            // 4) Events for those registrations
            var registeredEventIds = registrations
                .Select(p => p.Event_Id)
                .Distinct()
                .ToList();

            model.RegisteredEvents = allEvents
                .Where(e => registeredEventIds.Contains(e.Id))
                .OrderBy(e => e.Start_Time)
                .ToList();

            return View(model);
        }
    }
}


