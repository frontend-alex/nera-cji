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
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            // Look up user in DB to get full name
            var user = await _dbContext.users
                .FirstOrDefaultAsync(u => u.email == userEmail);

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

            var model = new DashboardViewModel {
                UserName = userName,
                UserEmail = userEmail
            };

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

            // Recent Activity: Last event registered
            var lastRegistration = await _dbContext.event_participants
                .Where(p => p.User_Id == user.Id && (p.Status == null || p.Status == "registered"))
                .OrderByDescending(p => p.Registered_At)
                .FirstOrDefaultAsync();

            if (lastRegistration != null)
            {
                model.LastEventRegistered = await _dbContext.events
                    .FirstOrDefaultAsync(e => e.Id == lastRegistration.Event_Id);
            }

            // Recent Activity: Last event created
            model.LastEventCreated = await _dbContext.events
                .Where(e => e.Created_By == user.Id)
                .OrderByDescending(e => e.Created_At)
                .FirstOrDefaultAsync();

            // Recent Activity: Recent notifications (last 5)
            model.RecentNotifications = await _dbContext.notifications
                .Where(n => n.User_Id == user.Id)
                .OrderByDescending(n => n.Created_At)
                .Take(5)
                .ToListAsync();

            return View(model);
        }
    }
}


