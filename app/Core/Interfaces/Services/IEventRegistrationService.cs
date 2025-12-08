using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nera_cji.Interfaces.Services {
    public interface IEventRegistrationService {
        Task<bool> RegisterAsync(int eventId, int userId);
        Task<bool> IsRegisteredAsync(int eventId, int userId);
        Task<int> GetRegisteredCountAsync(int eventId);
        Task<bool> UnregisterAsync(int eventId, int userId);

    }
}
