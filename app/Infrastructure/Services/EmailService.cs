using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using nera_cji.Interfaces.Services;

namespace nera_cji.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body, Attachment attachment = null)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var writeToDisk = emailSettings.GetValue<bool>("WriteToDisk");
            var pickupDirectory = emailSettings.GetValue<string>("PickupDirectoryLocation") ?? "mail_pickup";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress("no-reply@nera.com");
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                if (attachment != null)
                {
                    message.Attachments.Add(attachment);
                }

                using (var client = new SmtpClient())
                {
                    if (writeToDisk)
                    {
                        var path = Path.Combine(AppContext.BaseDirectory, pickupDirectory);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                        client.PickupDirectoryLocation = path;
                        client.Host = "localhost"; 
                    }
                    else
                    {
                        var host = emailSettings["Host"];
                        var port = int.Parse(emailSettings["Port"] ?? "587");
                        var username = emailSettings["Username"];
                        var password = emailSettings["Password"];
                        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

                        client.Host = host;
                        client.Port = port;
                        client.EnableSsl = enableSsl;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new System.Net.NetworkCredential(username, password);
                    }

                    await client.SendMailAsync(message);
                }
            }
        }
    }
}
