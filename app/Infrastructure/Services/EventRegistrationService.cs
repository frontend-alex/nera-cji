using System;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using Microsoft.EntityFrameworkCore;
using nera_cji.Interfaces.Services;
using nera_cji.Models;
using System.IO;
using System.Net.Mail;

namespace nera_cji.Infrastructure.Services {
    public class EventRegistrationService : IEventRegistrationService {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IQrCodeService _qrCodeService;

        public EventRegistrationService(ApplicationDbContext dbContext, IEmailService emailService, IQrCodeService qrCodeService) {
            _dbContext = dbContext;
            _emailService = emailService;
            _qrCodeService = qrCodeService;
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
                // Resend ticket if already registered
                await SendQrEmailAsync(userId, ev);
                return true; 
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

            // Send QR Code & Email
            await SendQrEmailAsync(userId, ev);

            return true;
        }

        private async Task SendQrEmailAsync(int userId, Event ev)
        {
             try
            {
                var user = await _dbContext.users.FindAsync(userId);
                if (user != null && !string.IsNullOrEmpty(user.email))
                {
                    var qrData = $"nera-event:{ev.Id}-user:{userId}-timestamp:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    var qrBytes = await _qrCodeService.GenerateQrCodeAsync(qrData);

                    using (var ms = new MemoryStream(qrBytes))
                    {
                        var attachment = new Attachment(ms, "qrcode.png", "image/png");
                        var subject = $"Registration Confirmed: {ev.Title}";
                        var body = $"<h1>You are registered!</h1><p>Event: {ev.Title}</p><p>Please find your ticket QR code attached.</p>";

                        await _emailService.SendEmailAsync(user.email, subject, body, attachment);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email/QR: {ex.Message}");
            }
        }

        public async Task<bool> IsRegisteredAsync(int eventId, int userId) {
            return await _dbContext.event_participants
                .AnyAsync(p => p.Event_Id == eventId
                            && p.User_Id == userId
                            && (p.Status == null || p.Status == "registered"));
        }
        public async Task<bool> UnregisterAsync(int eventId, int userId) {
            // Find all active registrations for this user + event
            var registrations = await _dbContext.event_participants
                .Where(p => p.Event_Id == eventId
                         && p.User_Id == userId
                         && (p.Status == null || p.Status == "registered"))
                .ToListAsync();

            if (registrations.Count == 0) {
                // Nothing to remove
                return false;
            }

            _dbContext.event_participants.RemoveRange(registrations);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetRegisteredCountAsync(int eventId) {
            return await _dbContext.event_participants
                .CountAsync(p => p.Event_Id == eventId
                              && (p.Status == null || p.Status == "registered"));
        }
    }
}

