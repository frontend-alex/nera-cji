using System.Net.Mail;
using System.Threading.Tasks;

namespace nera_cji.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, Attachment attachment = null);
    }
}
