namespace nera_cji.Services;

using App.Core;
using Microsoft.EntityFrameworkCore;
using nera_cji.Interfaces.Services;
using nera_cji.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EventService : IEventService {
    private readonly ApplicationDbContext _context;

    public EventService(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default) {
        return await _context.events
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default) {
        var events = await _context.events
            .OrderByDescending(e => e.Start_Time)
            .ToListAsync(cancellationToken);
        return events.AsReadOnly();
    }

    public async Task<Event> CreateAsync(Event eventEntity, CancellationToken cancellationToken = default) {
        eventEntity.Id = 0;
        eventEntity.Created_At = DateTime.UtcNow;
        eventEntity.Updated_At = DateTime.UtcNow;
        
        _context.events.Add(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return eventEntity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) {
        var eventEntity = await GetByIdAsync(id, cancellationToken);
        if (eventEntity == null) {
            return false;
        }

        _context.events.Remove(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task<Event> UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default) {
        eventEntity.Updated_At = DateTime.UtcNow;
        
        _context.events.Update(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return eventEntity;
    }
}

