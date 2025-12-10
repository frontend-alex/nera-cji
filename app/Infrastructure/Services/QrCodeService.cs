using System.Net.Http;
using System.Threading.Tasks;
using nera_cji.Interfaces.Services;

namespace nera_cji.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly HttpClient _httpClient;

        public QrCodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<byte[]> GenerateQrCodeAsync(string data)
        {
            // Using goqr.me API
            var url = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={System.Web.HttpUtility.UrlEncode(data)}";
            return await _httpClient.GetByteArrayAsync(url);
        }
    }
}
