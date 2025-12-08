using System;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using Microsoft.EntityFrameworkCore;
using nera_cji.Interfaces.Services;
using nera_cji.Models;

namespace nera_cji.Infrastructure.Services {
    public class EventRegistrationService : IEventRegistrationService {
        private readonly ApplicationDbContext _dbContext;

        public EventRegistrationService(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<bool> RegisterAsync(int eventId, int userId) {
            var ev = await _dbContext.events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null) {
                return false; // no such event
            }

            // Already registered?
            var alreadyRegistered = ev.Participants.Any(p =>
                p.User_Id == userId &&
                (p.Status == null || p.Status == "registered"));

            if (alreadyRegistered) {
                return true; // treat as success
            }

            // Capacity check
            var capacity = ev.Max_Participants ?? 0;
            var registeredCount = ev.Participants.Count(p =>
                p.Status == null || p.Status == "registered");

            if (capacity > 0 && registeredCount >= capacity) {
                return false; // full
            }

            var participant = new EventParticipant {
                Event_Id = eventId,
                User_Id = userId,
                Registered_At = DateTime.UtcNow,
                Status = "registered"
            };

            _dbContext.event_participants.Add(participant);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsRegisteredAsync(int eventId, int userId) {
            return await _dbContext.event_participants
                .AnyAsync(p => p.Event_Id == eventId
                            && p.User_Id == userId
                            && (p.Status == null || p.Status == "registered"));
        }

        public async Task<int> GetRegisteredCountAsync(int eventId) {
            return await _dbContext.event_participants
                .CountAsync(p => p.Event_Id == eventId
                              && (p.Status == null || p.Status == "registered"));
        }
    }
}

