namespace nera_cji.Services;

using nera_cji.Interfaces.Services;
using nera_cji.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class InMemoryEventService : IEventService {
    private readonly List<Event> _events;
    private int _nextId = 1;
    private readonly object _lock = new object();

    public InMemoryEventService() {
        _events = new List<Event>
        {
            new Event {
                Id = _nextId++,
                Title = "Annual Tech Conference 2025",
                Description = "Join us for the biggest tech conference of the year featuring keynote speakers from leading tech companies.",
                Location = "Convention Center, Main Hall",
                StartTime = DateTime.UtcNow.AddDays(7),
                EndTime = DateTime.UtcNow.AddDays(7).AddHours(8),
                CreatedBy = 1,
                MaxParticipants = 500,
                Status = "Published",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Event {
                Id = _nextId++,
                Title = "Team Building Workshop",
                Description = "Interactive workshop focused on improving team collaboration and communication skills.",
                Location = "Building A, Room 301",
                StartTime = DateTime.UtcNow.AddDays(3),
                EndTime = DateTime.UtcNow.AddDays(3).AddHours(4),
                CreatedBy = 1,
                MaxParticipants = 30,
                Status = "Published",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Event {
                Id = _nextId++,
                Title = "Networking Mixer",
                Description = "Casual networking event for professionals to connect and share ideas over drinks and appetizers.",
                Location = "Rooftop Lounge",
                StartTime = DateTime.UtcNow.AddDays(14),
                EndTime = DateTime.UtcNow.AddDays(14).AddHours(3),
                CreatedBy = 1,
                MaxParticipants = 100,
                Status = "Published",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Event {
                Id = _nextId++,
                Title = "Product Launch Event",
                Description = "Exclusive first look at our new product line with live demonstrations and Q&A sessions.",
                Location = "Auditorium",
                StartTime = DateTime.UtcNow.AddDays(21),
                EndTime = DateTime.UtcNow.AddDays(21).AddHours(5),
                CreatedBy = 1,
                MaxParticipants = 200,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Event {
                Id = _nextId++,
                Title = "Training Session: New Software Tools",
                Description = "Hands-on training for the new project management software being rolled out company-wide.",
                Location = "Training Room B",
                StartTime = DateTime.UtcNow.AddDays(5),
                EndTime = DateTime.UtcNow.AddDays(5).AddHours(2),
                CreatedBy = 1,
                MaxParticipants = 25,
                Status = "Published",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            }
        };
    }

    public Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default) {
        lock (_lock) {
            var eventEntity = _events.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(eventEntity);
        }
    }

    public Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default) {
        lock (_lock) {
            var events = _events
                .OrderByDescending(e => e.StartTime)
                .ToList()
                .AsReadOnly();
            return Task.FromResult<IReadOnlyCollection<Event>>(events);
        }
    }

    public Task<Event> CreateAsync(Event eventEntity, CancellationToken cancellationToken = default) {
        lock (_lock) {
            eventEntity.Id = _nextId++;
            eventEntity.CreatedAt = DateTime.UtcNow;
            eventEntity.UpdatedAt = DateTime.UtcNow;
            
            _events.Add(eventEntity);
            
            return Task.FromResult(eventEntity);
        }
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) {
        lock (_lock) {
            var eventEntity = _events.FirstOrDefault(e => e.Id == id);
            if (eventEntity == null) {
                return Task.FromResult(false);
            }

            _events.Remove(eventEntity);
            return Task.FromResult(true);
        }
    }

    public Task<Event> UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default) {
        lock (_lock) {
            var existingEvent = _events.FirstOrDefault(e => e.Id == eventEntity.Id);
            if (existingEvent != null) {
                _events.Remove(existingEvent);
            }
            
            eventEntity.UpdatedAt = DateTime.UtcNow;
            _events.Add(eventEntity);
            
            return Task.FromResult(eventEntity);
        }
    }
}
