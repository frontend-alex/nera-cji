using System.Threading.Tasks;

namespace nera_cji.Interfaces.Services
{
    public interface IQrCodeService
    {
        Task<byte[]> GenerateQrCodeAsync(string data);
    }
}
