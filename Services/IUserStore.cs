using nera_cji.Models;

namespace nera_cji.Services
{
    public interface IUserStore
    {
        Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
