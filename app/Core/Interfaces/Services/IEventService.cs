namespace nera_cji.Interfaces.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using nera_cji.Models;

public interface IEventService {
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Event> CreateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteForceAsync(int id, CancellationToken cancellationToken = default);
    Task<Event> UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
}

