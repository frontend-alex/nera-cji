namespace nera_cji.Interfaces.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using nera_cji.Models;

public interface IUserService {
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default);
}


